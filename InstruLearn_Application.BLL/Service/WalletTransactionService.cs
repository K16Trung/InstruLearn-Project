using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.WalletTransaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<WalletTransactionService> _logger;
        private readonly ISystemConfigurationService _configService;

        public WalletTransactionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<WalletTransactionService> logger,
            ISystemConfigurationService configService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _configService = configService;
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
                _logger.LogError(ex, "Error retrieving transactions");
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
                _logger.LogError(ex, $"Error retrieving transactions by wallet ID: {walletId}");
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
                _logger.LogError(ex, $"Error retrieving transactions by learner ID: {learnerId}");
                return new List<WalletTransactionDTO>();
            }
        }

        private async Task<List<WalletTransactionDTO>> EnrichTransactionsWithPaymentInfoAsync(IEnumerable<Model.Models.WalletTransaction> transactions)
        {
            var transactionDtos = _mapper.Map<List<WalletTransactionDTO>>(transactions);

            try
            {
                // Get all transaction IDs
                var transactionIds = transactions.Select(t => t.TransactionId).ToList();

                // Get all payments linked to these transactions
                var payments = await _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => transactionIds.Contains(p.TransactionId))
                    .ToListAsync();

                // Get learning registrations
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

                // Find all learning registrations created in the same timeframe as the transactions
                // This will help identify class join and registration fee transactions
                var recentRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(lr => lr.RequestDate >= DateTime.UtcNow.AddDays(-30))
                    .ToListAsync();

                // Check each transaction
                foreach (var dto in transactionDtos)
                {
                    var originalTransaction = transactions.FirstOrDefault(t => t.TransactionId == dto.TransactionId);
                    var payment = payments.FirstOrDefault(p => p.TransactionId == dto.TransactionId);

                    // Check for application fee payments first - this takes highest priority
                    if (payment != null && payment.PaymentFor == PaymentFor.ApplicationFee)
                    {
                        dto.PaymentType = "Phí đăng ký";
                        continue;
                    }

                    // DIRECT METHOD: Check if transaction matches a learning registration that has ClassId
                    // When a transaction has an associated learning registration with ClassId, it's a class enrollment
                    // With ClassId -> Center class enrollment
                    var classRegistrationMatch = recentRegistrations.FirstOrDefault(lr =>
                        lr.ClassId.HasValue && /* ClassId has value */
                        originalTransaction != null &&
                        lr.RequestDate != default &&
                        Math.Abs((originalTransaction.TransactionDate - lr.RequestDate).TotalMinutes) < 5); // 5 minute window

                    // DIRECT METHOD: Check if transaction matches a learning registration without ClassId 
                    // When a transaction has an associated learning registration without ClassId, it's a one-on-one registration fee
                    // Without ClassId -> Registration fee
                    var oneOnOneRegistrationMatch = recentRegistrations.FirstOrDefault(lr =>
                        !lr.ClassId.HasValue && /* ClassId is null */
                        originalTransaction != null &&
                        lr.RequestDate != default &&
                        Math.Abs((originalTransaction.TransactionDate - lr.RequestDate).TotalMinutes) < 5); // 5 minute window

                    if (classRegistrationMatch != null)
                    {
                        // This is definitely a class enrollment payment
                        dto.PaymentType = "Tham gia lớp học trung tâm";
                        continue;
                    }
                    else if (oneOnOneRegistrationMatch != null && dto.Amount == 50000) // Standard deposit amount
                    {
                        // This is definitely a registration fee for one-on-one learning
                        dto.PaymentType = "Phí đăng ký";
                        continue;
                    }

                    // If we have a payment record, use its information
                    if (payment != null)
                    {
                        if (payment.PaymentFor == PaymentFor.LearningRegistration)
                        {
                            var registration = learningRegistrations.FirstOrDefault(lr => lr.LearningRegisId == payment.PaymentId);

                            if (registration != null)
                            {
                                // Check if this is a class registration (has ClassId)
                                if (registration.ClassId.HasValue)
                                {
                                    dto.PaymentType = "Tham gia lớp học trung tâm";
                                    continue;
                                }
                                // If ClassId is null, this is a one-on-one registration fee
                                else if (!registration.ClassId.HasValue)
                                {
                                    dto.PaymentType = "Phí đăng ký";
                                    continue;
                                }

                                // If we got here, process other payment amount checks
                                if (registration.Price.HasValue)
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
                        else
                        {
                            dto.PaymentType = payment.PaymentFor.ToString();
                        }
                    }
                    else
                    {
                        // Special handling for 50000 VND deposits without payment records
                        if (originalTransaction != null &&
                            originalTransaction.TransactionType == TransactionType.Payment &&
                            Math.Abs(originalTransaction.Amount - 50000) < 0.1m)
                        {
                            // This is very likely a registration deposit
                            dto.PaymentType = "Phí đăng ký";
                            continue;
                        }

                        switch (dto.TransactionType?.ToLower())
                        {
                            case "addfunds":
                            case "addfun":
                            case "addfund":
                            case "addfuns":
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

                    if (dto.PaymentType == "AddFuns")
                    {
                        dto.PaymentType = "Nạp tiền";
                    }
                }

                return transactionDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching transactions");

                foreach (var dto in transactionDtos)
                {
                    if (string.IsNullOrEmpty(dto.PaymentType))
                    {
                        dto.PaymentType = dto.TransactionType ?? "Không xác định";
                    }

                    if (dto.PaymentType == "AddFuns")
                    {
                        dto.PaymentType = "Nạp tiền";
                    }
                }

                return transactionDtos;
            }
        }
    }
}