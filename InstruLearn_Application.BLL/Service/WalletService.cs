using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Configuration;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Migrations;
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
            string baseUrl = "http://localhost:3000/profile?";

            PaymentData paymentData = new PaymentData(
            orderCode: orderCode,
            amount: (int)amount,
            description: $"Add Funds #{learnerId}",
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

            transaction.OrderCode = createPayment.orderCode;
            await _unitOfWork.SaveChangeAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await PollTransactionStatusAsync(transactionId, createPayment.orderCode);
                }
                catch (Exception ex)
                {
                    // Log but don't throw
                    // _logger.LogError(ex, $"Error polling transaction status: {ex.Message}");
                }
            });

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

            string transactionId = Guid.NewGuid().ToString();

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

            var vnpayRequest = new VnpayPaymentRequest
            {
                Amount = amount,
                OrderDescription = $"Add funds to wallet for user #{learnerId}",
                LearnerId = learnerId,
                TransactionId = transactionId,
                SuccessUrl = _vnpaySettings.SuccessUrl,
                FailureUrl = _vnpaySettings.FailureUrl
            };

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

            if (transaction.Status == TransactionStatus.Complete)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Transaction is already completed"
                };
            }

            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                transaction.Status = TransactionStatus.Complete;

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
        public async Task<ResponseDTO> FailPaymentAsync(string transactionId)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository
                .GetTransactionWithWalletAsync(transactionId);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Transaction not found" };
            }

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
                transaction.Status = TransactionStatus.Failed;

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

            var walletDto = _mapper.Map<WalletDTO>(wallet);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Wallet retrieved successfully",
                Data = walletDto
            };
        }

        public async Task<ResponseDTO> UpdatePaymentStatusByOrderCodeAsync(long orderCode, string status)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository.GetOrderCodeWithWalletAsync(orderCode);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = $"Transaction not found for OrderCode: {orderCode}" };
            }

            if (transaction.Status == TransactionStatus.Complete || transaction.Status == TransactionStatus.Failed)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Cannot update transaction that is already in {transaction.Status} status"
                };
            }

            if (status == "PAID")
            {
                using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    transaction.Status = TransactionStatus.Complete;

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
                            OrderCode = transaction.OrderCode,
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
            else if (status == "CANCELLED" || status == "FAILED")
            {
                try
                {
                    transaction.Status = TransactionStatus.Failed;

                    await _unitOfWork.SaveChangeAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Payment marked as failed",
                        Data = new
                        {
                            TransactionId = transaction.TransactionId,
                            OrderCode = transaction.OrderCode,
                            Status = "Failed"
                        }
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseDTO { IsSucceed = false, Message = $"Error updating payment status: {ex.Message}" };
                }
            }
            else
            {
                // Unknown status
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Unknown payment status: {status}"
                };
            }
        }

        public async Task<ResponseDTO> ProcessVnpayReturnAsync(VnpayPaymentResponse paymentResponse)
        {
            if (paymentResponse == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Invalid payment response" };
            }

            var transaction = await _unitOfWork.WalletTransactionRepository
                .GetTransactionWithWalletAsync(paymentResponse.TxnRef);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = $"Transaction not found for ID: {paymentResponse.TxnRef}" };
            }

            if (transaction.Status == TransactionStatus.Complete || transaction.Status == TransactionStatus.Failed)
            {
                return new ResponseDTO
                {
                    IsSucceed = transaction.Status == TransactionStatus.Complete,
                    Message = $"Transaction is already in {transaction.Status} status",
                    Data = new
                    {
                        TransactionId = transaction.TransactionId,
                        Status = transaction.Status.ToString(),
                        SuccessUrl = _vnpaySettings.SuccessUrl,
                        FailureUrl = _vnpaySettings.FailureUrl
                    }
                };
            }

            if (paymentResponse.ResponseCode == "00")
            {
                using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    transaction.Status = TransactionStatus.Complete;
                    transaction.Wallet.Balance += transaction.Amount;
                    transaction.Wallet.UpdateAt = DateTime.Now;

                    await _unitOfWork.SaveChangeAsync();
                    await dbTransaction.CommitAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "VNPay payment completed successfully",
                        Data = new
                        {
                            TransactionId = transaction.TransactionId,
                            Amount = transaction.Amount,
                            NewBalance = transaction.Wallet.Balance,
                            SuccessUrl = _vnpaySettings.SuccessUrl,
                            FailureUrl = _vnpaySettings.FailureUrl
                        }
                    };
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Error completing payment: {ex.Message}",
                        Data = new
                        {
                            FailureUrl = _vnpaySettings.FailureUrl
                        }
                    };
                }
            }
            else
            {
                try
                {
                    transaction.Status = TransactionStatus.Failed;
                    await _unitOfWork.SaveChangeAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Payment failed: {paymentResponse.Message}",
                        Data = new
                        {
                            TransactionId = transaction.TransactionId,
                            Status = "Failed",
                            ResponseCode = paymentResponse.ResponseCode,
                            ResponseMessage = paymentResponse.Message,
                            FailureUrl = _vnpaySettings.FailureUrl
                        }
                    };
                }
                catch (Exception ex)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Error updating payment status: {ex.Message}",
                        Data = new
                        {
                            FailureUrl = _vnpaySettings.FailureUrl
                        }
                    };
                }
            }
        }

        private async Task PollTransactionStatusAsync(string transactionId, long orderCode)
        {
            // First poll after 2 minutes
            await Task.Delay(TimeSpan.FromMinutes(2));

            // Create PayOS instance
            PayOS payOS = new PayOS(_payOSSettings.ClientId, _payOSSettings.ApiKey, _payOSSettings.ChecksumKey);

            // Total number of attempts (checking every 5 mins for 30 mins)
            int maxAttempts = 6;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    // Get transaction from database first to check if it's already processed
                    var transaction = await _unitOfWork.WalletTransactionRepository.GetTransactionWithWalletAsync(transactionId);

                    // If transaction is already in final state, no need to continue polling
                    if (transaction != null && (transaction.Status == TransactionStatus.Complete ||
                                               transaction.Status == TransactionStatus.Failed))
                    {
                        return;
                    }

                    // Get payment status from PayOS
                    var paymentInfo = await payOS.getPaymentLinkInformation(orderCode);

                    if (paymentInfo.status == "PAID")
                    {
                        // Update wallet balance
                        await UpdatePaymentStatusAsync(transactionId);
                        return;
                    }
                    else if (paymentInfo.status == "CANCELLED" || paymentInfo.status == "FAILED")
                    {
                        await FailPaymentAsync(transactionId);
                        return;
                    }
                }
                catch
                {
                    // Ignore errors and continue polling
                }

                // Wait 5 minutes before next check
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }
    }
}
