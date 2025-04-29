using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.WalletTransaction;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class WalletTransactionService : IWalletTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WalletTransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<WalletTransactionDTO>> GetAllTransactionsAsync()
        {
            try
            {
                var transactions = await _unitOfWork.WalletTransactionRepository.GetAllTransactionsAsync();
                return await EnrichTransactionsWithPaymentInfoAsync(transactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving transactions: {ex.Message}");
                return new List<WalletTransactionDTO>();
            }
        }

        public async Task<List<WalletTransactionDTO>> GetTransactionsByWalletIdAsync(int walletId)
        {
            try
            {
                var transactions = await _unitOfWork.WalletTransactionRepository.GetTransactionsByWalletIdAsync(walletId);
                return await EnrichTransactionsWithPaymentInfoAsync(transactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving transactions by wallet ID: {ex.Message}");
                return new List<WalletTransactionDTO>();
            }
        }

        public async Task<List<WalletTransactionDTO>> GetTransactionsByLearnerIdAsync(int learnerId)
        {
            try
            {
                var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == learnerId);
                if (wallet == null)
                {
                    return new List<WalletTransactionDTO>();
                }

                var transactions = await _unitOfWork.WalletTransactionRepository.GetTransactionsByWalletIdAsync(wallet.WalletId);
                return await EnrichTransactionsWithPaymentInfoAsync(transactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving transactions by learner ID: {ex.Message}");
                return new List<WalletTransactionDTO>();
            }
        }

        private async Task<List<WalletTransactionDTO>> EnrichTransactionsWithPaymentInfoAsync(IEnumerable<Model.Models.WalletTransaction> transactions)
        {
            var transactionDtos = _mapper.Map<List<WalletTransactionDTO>>(transactions);

            try
            {
                var transactionIds = transactions.Select(t => t.TransactionId).ToList();
                var payments = await _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => transactionIds.Contains(p.TransactionId))
                    .ToListAsync();

                var learningRegistrationIds = payments
                    .Where(p => p.PaymentFor == PaymentFor.LearningRegistration)
                    .Select(p => p.PaymentId)
                    .ToList();

                var learningRegistrations = new List<Learning_Registration>();
                if (learningRegistrationIds.Any())
                {
                    learningRegistrations = await _unitOfWork.LearningRegisRepository
                        .GetQuery()
                        .Where(lr => learningRegistrationIds.Contains(lr.LearningRegisId))
                        .ToListAsync();
                }

                foreach (var dto in transactionDtos)
                {
                    var originalTransaction = transactions.FirstOrDefault(t => t.TransactionId == dto.TransactionId);
                    var payment = payments.FirstOrDefault(p => p.TransactionId == dto.TransactionId);

                    if (payment != null)
                    {
                        if (payment.PaymentFor == PaymentFor.LearningRegistration)
                        {
                            var isClassPayment = originalTransaction != null &&
                                (originalTransaction.Amount < 200000);

                            var regWithClass = learningRegistrations.FirstOrDefault(
                                lr => lr.LearningRegisId == payment.PaymentId && lr.ClassId.HasValue);

                            if (regWithClass != null)
                            {
                                dto.PaymentType = "Tham gia lớp học trung tâm";
                                continue;
                            }

                            var registration = learningRegistrations.FirstOrDefault(lr => lr.LearningRegisId == payment.PaymentId);

                            if (registration != null && registration.Price.HasValue)
                            {
                                decimal totalPrice = registration.Price.Value;
                                decimal fortyPercent = Math.Round(totalPrice * 0.4m, 2);
                                decimal sixtyPercent = Math.Round(totalPrice * 0.6m, 2);

                                if (Math.Abs(payment.AmountPaid - fortyPercent) < 0.1m)
                                {
                                    dto.PaymentType = "Thanh toán 40% học phí";
                                }
                                else if (Math.Abs(payment.AmountPaid - sixtyPercent) < 0.1m)
                                {
                                    dto.PaymentType = "Thanh toán 60% học phí";
                                }
                                else
                                {
                                    dto.PaymentType = "Đăng ký học";
                                }
                            }
                            else
                            {
                                dto.PaymentType = "Đăng ký học";
                            }
                        }
                        else if (payment.PaymentFor == PaymentFor.Online_Course)
                        {
                            dto.PaymentType = "Khóa học trực tuyến";
                        }
                        else if (payment.PaymentFor == PaymentFor.AddFuns)
                        {
                            dto.PaymentType = "Nạp tiền";
                        }
                        else if (payment.PaymentFor == PaymentFor.ApplicationFee)
                        {
                            dto.PaymentType = "Phí đăng ký";
                        }
                        else
                        {
                            dto.PaymentType = payment.PaymentFor.ToString();
                        }
                    }
                    else
                    {
                        if (dto.TransactionType?.ToLower() == "payment" && Math.Abs(dto.Amount - 50000) < 0.1m)
                        {
                            dto.PaymentType = "Phí đăng ký";
                        }
                        else if (dto.TransactionType?.ToLower() == "payment" && dto.Amount < 200000)
                        {
                            dto.PaymentType = "Tham gia lớp học trung tâm";
                        }
                        else
                        {
                            switch (dto.TransactionType?.ToLower())
                            {
                                case "addfunds":
                                case "addfun":
                                case "addfund":
                                    dto.PaymentType = "Nạp tiền";
                                    break;
                                case "payment":
                                    dto.PaymentType = "Thanh toán";
                                    break;
                                default:
                                    dto.PaymentType = dto.TransactionType ?? "Không xác định";
                                    break;
                            }
                        }
                    }
                }

                return transactionDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enriching transactions: {ex.Message}");

                foreach (var dto in transactionDtos)
                {
                    if (string.IsNullOrEmpty(dto.PaymentType))
                    {
                        dto.PaymentType = dto.TransactionType ?? "Không xác định";
                    }
                }

                return transactionDtos;
            }
        }
    }
}