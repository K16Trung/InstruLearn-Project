using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
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

                var learningRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(lr => lr.Price.HasValue)
                    .ToListAsync();

                foreach (var dto in transactionDtos)
                {
                    var payment = payments.FirstOrDefault(p => p.TransactionId == dto.TransactionId);

                    if (payment != null)
                    {
                        if (payment.PaymentFor == PaymentFor.LearningRegistration)
                        {
                            var matchingRegistration = learningRegistrations
                                .FirstOrDefault(lr =>
                                    lr.Price.HasValue &&
                                    (Math.Abs(payment.AmountPaid - lr.Price.Value * 0.4m) < 0.1m ||
                                     Math.Abs(payment.AmountPaid - lr.Price.Value * 0.6m) < 0.1m));

                            if (matchingRegistration != null)
                            {
                                decimal totalPrice = matchingRegistration.Price.Value;
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
                                    dto.PaymentType = matchingRegistration.ClassId == null
                                        ? PaymentType.OneOnOne.ToString()
                                        : PaymentType.Center.ToString();
                                }
                            }
                            else
                            {
                                dto.PaymentType = "LearningRegistration";
                            }
                        }
                        else if (payment.PaymentFor == PaymentFor.Online_Course)
                        {
                            dto.PaymentType = PaymentType.OnlineCourse.ToString();
                        }
                        else if (payment.PaymentFor == PaymentFor.AddFuns)
                        {
                            dto.PaymentType = "AddFunds";
                        }
                        else if (payment.PaymentFor == PaymentFor.ApplicationFee)
                        {
                            dto.PaymentType = PaymentType.ApplicationFee.ToString();
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
                            dto.PaymentType = PaymentType.ApplicationFee.ToString();
                        }
                        else
                        {
                            switch (dto.TransactionType?.ToLower())
                            {
                                case "addfunds":
                                    dto.PaymentType = "AddFunds";
                                    break;
                                case "payment":
                                    dto.PaymentType = "Payment";
                                    break;
                                default:
                                    dto.PaymentType = dto.TransactionType ?? "Unknown";
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
                        dto.PaymentType = dto.TransactionType ?? "Unknown";
                    }
                }

                return transactionDtos;
            }
        }
    }
}