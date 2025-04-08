using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Configuration;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Vnpay;
using InstruLearn_Application.Model.Models.DTO.Wallet;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class WalletService : IWalletService
    {
        private readonly PayOSSettings _payOSSettings;
        private readonly VnpaySettings _vnpaySettings;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnpayService _vnpayService;

        public WalletService(PayOSSettings payOSSettings, VnpaySettings vnpaySettings, IUnitOfWork unitOfWork, IMapper mapper, IVnpayService vnpayService)
        {
            _payOSSettings = payOSSettings;
            _vnpaySettings = vnpaySettings;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _vnpayService = vnpayService;
        }

        public async Task<ResponseDTO> AddFundsToWallet(int learnerId, decimal amount)
        {
            if (amount <= 0)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Amount must be greater than zero." };
            }

            var wallet = await _unitOfWork.WalletRepository.FirstOrDefaultAsync(w => w.LearnerId == learnerId);
            if (wallet == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Wallet not found" };
            }

            long orderCode = new Random().Next(100000, 999999);

            // Generate a unique transaction ID
            string transactionId = Guid.NewGuid().ToString();

            // Create a wallet transaction
            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                TransactionId = transactionId,
                //OrderCode = orderCode,
                TransactionType = TransactionType.AddFuns,
                Status = TransactionStatus.Pending,
                TransactionDate = DateTime.Now
            };

            await _unitOfWork.WalletTransactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangeAsync();

            // Use PayOS to generate a payment link
            PayOS payOS = new PayOS(_payOSSettings.ClientId, _payOSSettings.ApiKey, _payOSSettings.ChecksumKey);
            List<ItemData> items = new List<ItemData>
            {
                new ItemData("Add Funds to Wallet", 1, (int)amount)
            };
            string baseUrl = "https://instrulearnapplication2025-h7hfdte3etdth7av.southeastasia-01.azurewebsites.net";

            PaymentData paymentData = new PaymentData(
            orderCode: orderCode,
            amount: (int)amount,
            description: $"Add Funds to Wallet for Learner #{learnerId}",
            items: items,
            cancelUrl: $"{baseUrl}/api/payos/result?id={transactionId}&cancel=true",
            returnUrl: $"{baseUrl}/api/payos/result?id={transactionId}&cancel=false"
            );

            var createPayment = await payOS.createPaymentLink(paymentData);

            if (createPayment == null || string.IsNullOrEmpty(createPayment.checkoutUrl))
            {
                transaction.Status = TransactionStatus.Failed;
                await _unitOfWork.SaveChangeAsync();
                return new ResponseDTO { IsSucceed = false, Message = "Failed to generate payment link" };
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Payment link created",
                Data = new
                {
                    TransactionId = transaction.TransactionId,
                    Amount = transaction.Amount,
                    Status = transaction.Status.ToString(),
                    PaymentUrl = createPayment.checkoutUrl
                }
            };
        }

        public async Task<ResponseDTO> AddFundsWithVnpay(int learnerId, decimal amount, string ipAddress)
        {
            if (amount <= 0)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Amount must be greater than zero." };
            }

            var wallet = await _unitOfWork.WalletRepository.FirstOrDefaultAsync(w => w.LearnerId == learnerId);
            if (wallet == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Wallet not found" };
            }

            // Generate a unique transaction ID
            string transactionId = Guid.NewGuid().ToString();

            // Create a wallet transaction
            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                TransactionId = transactionId,
                TransactionType = TransactionType.AddFuns,
                Status = TransactionStatus.Pending,
                TransactionDate = DateTime.Now
            };

            await _unitOfWork.WalletTransactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangeAsync();

            // Create VNPay payment request
            var vnpayRequest = new VnpayPaymentRequest
            {
                OrderId = string.Empty, // Not using OrderId
                Amount = amount,
                OrderDescription = $"Add funds to wallet for user #{learnerId}",
                LearnerId = learnerId,
                TransactionId = transactionId
            };

            // Generate payment URL
            string paymentUrl = _vnpayService.CreatePaymentUrl(vnpayRequest, ipAddress);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "VNPay payment link created",
                Data = new
                {
                    TransactionId = transaction.TransactionId,
                    Amount = transaction.Amount,
                    Status = transaction.Status.ToString(),
                    PaymentUrl = paymentUrl
                }
            };
        }

        public async Task<ResponseDTO> UpdatePaymentStatusAsync(string transactionId)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository
                .GetTransactionWithWalletAsync(transactionId);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Transaction not found" };
            }

            // Check if transaction is already completed
            if (transaction.Status == TransactionStatus.Complete)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Transaction is already completed"
                };
            }

            // Begin transaction to ensure atomicity
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update status to Complete
                transaction.Status = TransactionStatus.Complete;

                // Add the amount to the wallet balance
                transaction.Wallet.Balance += transaction.Amount;

                await _unitOfWork.SaveChangeAsync();
                await dbTransaction.CommitAsync();

                return new ResponseDTO { IsSucceed = true, Message = "Payment completed successfully" };
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return new ResponseDTO { IsSucceed = false, Message = $"Error completing payment: {ex.Message}" };
            }
        }

        public async Task<ResponseDTO> ProcessVnpayReturnAsync(VnpayPaymentResponse paymentResponse)
        {
            if (!paymentResponse.Success)
            {
                return new ResponseDTO { IsSucceed = false, Message = $"Payment failed: {paymentResponse.Message}" };
            }

            var transaction = await _unitOfWork.WalletTransactionRepository
                .GetTransactionWithWalletAsync(paymentResponse.TransactionId);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Transaction not found" };
            }

            // Check if transaction is already completed
            if (transaction.Status == TransactionStatus.Complete)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Transaction is already completed"
                };
            }

            // Begin transaction to ensure atomicity
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update status to Complete
                transaction.Status = TransactionStatus.Complete;

                // Add the amount to the wallet balance
                transaction.Wallet.Balance += transaction.Amount;
                transaction.Wallet.UpdateAt = DateTime.Now;

                await _unitOfWork.SaveChangeAsync();
                await dbTransaction.CommitAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Payment completed successfully",
                    Data = new
                    {
                        TransactionId = transaction.TransactionId,
                        Amount = transaction.Amount,
                        NewBalance = transaction.Wallet.Balance
                    }
                };
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return new ResponseDTO { IsSucceed = false, Message = $"Error completing payment: {ex.Message}" };
            }
        }

        public async Task<ResponseDTO> FailPaymentAsync(string transactionId)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository
                .GetTransactionWithWalletAsync(transactionId);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Transaction not found" };
            }

            // Check if transaction is already in a final state
            if (transaction.Status == TransactionStatus.Complete ||
                transaction.Status == TransactionStatus.Failed)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Cannot update transaction that is already in {transaction.Status} status"
                };
            }

            try
            {
                // Update status to Failed
                transaction.Status = TransactionStatus.Failed;

                // No balance update needed for failed transactions
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO { IsSucceed = true, Message = "Payment marked as failed" };
            }
            catch (Exception ex)
            {
                return new ResponseDTO { IsSucceed = false, Message = $"Error updating payment status: {ex.Message}" };
            }
        }

        public async Task<ResponseDTO> GetWalletByLearnerIdAsync(int learnerId)
        {
            var wallet = await _unitOfWork.WalletRepository.GetWalletByLearnerIdAsync(learnerId);

            if (wallet == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Wallet not found",
                    Data = null
                };
            }

            // Use AutoMapper to map Wallet entity to WalletDTO
            var walletDto = _mapper.Map<WalletDTO>(wallet);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Wallet retrieved successfully",
                Data = walletDto
            };
        }
    }
}
