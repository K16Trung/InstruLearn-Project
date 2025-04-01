using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Configuration;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(PayOSSettings payOSSettings, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _payOSSettings = payOSSettings;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            // Create a wallet transaction
            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                TransactionId = Guid.NewGuid().ToString(),
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

                PaymentData paymentData = new PaymentData(
                orderCode: orderCode,
                amount: (int)amount,
                description: "Add Funds to Wallet",
                items: items,
                cancelUrl: "https://www.facebook.com/FPTU.HCM",
                returnUrl: "https://fap.fpt.edu.vn/"
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

        public async Task<ResponseDTO> UpdatePaymentStatusAsync(string orderCode)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository
        .GetTransactionWithWalletAsync(orderCode);

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

        public async Task<ResponseDTO> FailPaymentAsync(string orderCode)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository
                .GetTransactionWithWalletAsync(orderCode);

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
