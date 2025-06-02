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
                return new ResponseDTO { IsSucceed = false, Message = "Số tiền phải lớn hơn không." };
            }

            var wallet = await _unitOfWork.WalletRepository.FirstOrDefaultAsync(w => w.LearnerId == learnerId);
            if (wallet == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy ví" };
            }

            long orderCode = new Random().Next(100000, 999999);

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

            PayOS payOS = new PayOS(_payOSSettings.ClientId, _payOSSettings.ApiKey, _payOSSettings.ChecksumKey);
            List<ItemData> items = new List<ItemData>
            {
                new ItemData("Add Funds to Wallet", 1, (int)amount)
            };
            string baseUrl = "https://instru-learn-cc1.vercel.app/payment-success";

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
                return new ResponseDTO { IsSucceed = false, Message = "Không thể tạo đường dẫn thanh toán" };
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
                }
            });

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã tạo đường dẫn thanh toán",
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
                return new ResponseDTO { IsSucceed = false, Message = "Số tiền phải lớn hơn không." };
            }

            var wallet = await _unitOfWork.WalletRepository.FirstOrDefaultAsync(w => w.LearnerId == learnerId);
            if (wallet == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy ví" };
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
                Message = "Đã tạo đường dẫn thanh toán VNPay",
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
                return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy giao dịch" };
            }

            if (transaction.Status == TransactionStatus.Complete)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Giao dịch đã hoàn thành"
                };
            }

            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                transaction.Status = TransactionStatus.Complete;

                transaction.Wallet.Balance += transaction.Amount;

                await _unitOfWork.SaveChangeAsync();
                await dbTransaction.CommitAsync();

                return new ResponseDTO { IsSucceed = true, Message = "Thanh toán đã hoàn tất thành công" };
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return new ResponseDTO { IsSucceed = false, Message = $"Lỗi khi hoàn tất thanh toán: {ex.Message}" };
            }
        }
        public async Task<ResponseDTO> FailPaymentAsync(string transactionId)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository
                .GetTransactionWithWalletAsync(transactionId);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy giao dịch" };
            }

            if (transaction.Status == TransactionStatus.Complete ||
                transaction.Status == TransactionStatus.Failed)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không thể cập nhật giao dịch đã ở trạng thái {transaction.Status}"
                };
            }

            try
            {
                transaction.Status = TransactionStatus.Failed;

                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO { IsSucceed = true, Message = "Thanh toán đã được đánh dấu là thất bại" };
            }
            catch (Exception ex)
            {
                return new ResponseDTO { IsSucceed = false, Message = $"Lỗi khi cập nhật trạng thái thanh toán: {ex.Message}" };
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
                    Message = "Không tìm thấy ví",
                    Data = null
                };
            }

            var walletDto = _mapper.Map<WalletDTO>(wallet);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Truy xuất ví thành công",
                Data = walletDto
            };
        }

        public async Task<ResponseDTO> UpdatePaymentStatusByOrderCodeAsync(long orderCode, string status)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository.GetOrderCodeWithWalletAsync(orderCode);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = $"Không tìm thấy giao dịch với mã đơn hàng: {orderCode}" };
            }

            if (transaction.Status == TransactionStatus.Complete || transaction.Status == TransactionStatus.Failed)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không thể cập nhật giao dịch đã ở trạng thái {transaction.Status}"
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
                        Message = "Thanh toán đã hoàn tất thành công",
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
                    return new ResponseDTO { IsSucceed = false, Message = $"Lỗi khi hoàn tất thanh toán: {ex.Message}" };
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
                        Message = "Thanh toán đã được đánh dấu là thất bại",
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
                    return new ResponseDTO { IsSucceed = false, Message = $"Lỗi khi cập nhật trạng thái thanh toán: {ex.Message}" };
                }
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Trạng thái thanh toán không xác định: {status}"
                };
            }
        }

        public async Task<ResponseDTO> ProcessVnpayReturnAsync(VnpayPaymentResponse paymentResponse)
        {
            if (paymentResponse == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Phản hồi thanh toán không hợp lệ" };
            }

            var transaction = await _unitOfWork.WalletTransactionRepository
                .GetTransactionWithWalletAsync(paymentResponse.TxnRef);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = $"Không tìm thấy giao dịch với ID: {paymentResponse.TxnRef}" };
            }

            if (transaction.Status == TransactionStatus.Complete || transaction.Status == TransactionStatus.Failed)
            {
                return new ResponseDTO
                {
                    IsSucceed = transaction.Status == TransactionStatus.Complete,
                    Message = $"Giao dịch đã ở trạng thái {transaction.Status}",
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
                        Message = "Thanh toán VNPay đã hoàn tất thành công",
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
                        Message = $"Lỗi khi hoàn tất thanh toán: {ex.Message}",
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
                        Message = $"Thanh toán thất bại: {paymentResponse.Message}",
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
                        Message = $"Lỗi khi cập nhật trạng thái thanh toán: {ex.Message}",
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
            await Task.Delay(TimeSpan.FromMinutes(2));

            PayOS payOS = new PayOS(_payOSSettings.ClientId, _payOSSettings.ApiKey, _payOSSettings.ChecksumKey);

            int maxAttempts = 6;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    var transaction = await _unitOfWork.WalletTransactionRepository.GetTransactionWithWalletAsync(transactionId);

                    if (transaction != null && (transaction.Status == TransactionStatus.Complete ||
                                               transaction.Status == TransactionStatus.Failed))
                    {
                        return;
                    }

                    var paymentInfo = await payOS.getPaymentLinkInformation(orderCode);

                    if (paymentInfo.status == "PAID")
                    {
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
                }

                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }
    }
}
