using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Payment;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> ProcessLearningRegisPaymentAsync(CreatePaymentDTO paymentDTO)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(paymentDTO.LearningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy đăng ký học tập." };
                }

                if (learningRegis.Price == null || learningRegis.Price <= 0)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Giá đăng ký học tập không hợp lệ." };
                }

                var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == learningRegis.LearnerId);
                if (wallet == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy ví tiền." };
                }

                decimal totalPrice = learningRegis.Price.Value;
                decimal requiredAmount = totalPrice * 0.4m;
                decimal remainingAmount = totalPrice * 0.6m;

                if (wallet.Balance < requiredAmount)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Số dư trong ví không đủ." };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                wallet.Balance -= requiredAmount;
                wallet.UpdateAt = DateTime.UtcNow;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);

                var walletTransaction = new WalletTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    WalletId = wallet.WalletId,
                    Amount = requiredAmount,
                    TransactionType = TransactionType.Payment,
                    Status = TransactionStatus.Complete,
                    TransactionDate = DateTime.UtcNow
                };
                await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);

                var payment = new Payment
                {
                    WalletId = wallet.WalletId,
                    TransactionId = walletTransaction.TransactionId,
                    AmountPaid = requiredAmount,
                    PaymentMethod = paymentDTO.PaymentMethod,
                    PaymentFor = PaymentFor.LearningRegistration,
                    Status = PaymentStatus.Completed
                };
                await _unitOfWork.PaymentsRepository.AddAsync(payment);

                learningRegis.Status = LearningRegis.Fourty;
                learningRegis.RemainingAmount = remainingAmount;
                learningRegis.HasPendingLearningPath = true;
                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                var existingNotifications = await _unitOfWork.StaffNotificationRepository
                    .GetQuery()
                    .Where(n => n.LearningRegisId == learningRegis.LearningRegisId &&
                                n.Type == NotificationType.CreateLearningPath &&
                                n.Status != NotificationStatus.Resolved)
                    .ToListAsync();

                foreach (var notification in existingNotifications)
                {
                    notification.Status = NotificationStatus.Resolved;
                    await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                }

                var schedules = DateTimeHelper.GenerateOneOnOneSchedules(learningRegis);
                await _unitOfWork.ScheduleRepository.AddRangeAsync(schedules);

                if (learningRegis.TeacherId.HasValue)
                {
                    var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(learningRegis.TeacherId.Value);
                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learningRegis.LearnerId);

                    if (teacher != null && learner != null)
                    {
                        var staffNotification = new StaffNotification
                        {
                            Title = "Lịch trình đã tạo - Thanh toán đã nhận",
                            Message = $"Học viên {learner.FullName} đã thanh toán 40% cho đơn đăng kí: {learningRegis.LearningRegisId}. " +
                                      $"Lịch trình giảng dạy của bạn đã được tạo và hiện có trong lịch của bạn.",
                            LearningRegisId = learningRegis.LearningRegisId,
                            LearnerId = learningRegis.LearnerId,
                            CreatedAt = DateTime.Now,
                            Status = NotificationStatus.Unread,
                            Type = NotificationType.SchedulesCreated
                        };

                        await _unitOfWork.StaffNotificationRepository.AddAsync(staffNotification);
                    }
                }

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                var paymentResponse = _mapper.Map<PaymentDTO>(payment);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Thanh toán 40% cho đăng ký học tập thành công.",
                    Data = paymentResponse
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO { IsSucceed = false, Message = "Thanh toán thất bại. " + ex.Message };
            }
        }


        public async Task<ResponseDTO> ProcessRemainingPaymentAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy đăng ký học tập." };
                }

                if (learningRegis.RemainingAmount == null || learningRegis.RemainingAmount <= 0)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Không cần thanh toán thêm." };
                }

                var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == learningRegis.LearnerId);
                if (wallet == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy ví tiền." };
                }

                if (wallet.Balance < learningRegis.RemainingAmount)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Số dư trong ví không đủ." };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                wallet.Balance -= learningRegis.RemainingAmount.Value;
                wallet.UpdateAt = DateTime.UtcNow;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);

                var walletTransaction = new WalletTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    WalletId = wallet.WalletId,
                    Amount = learningRegis.RemainingAmount.Value,
                    TransactionType = TransactionType.Payment,
                    Status = TransactionStatus.Complete,
                    TransactionDate = DateTime.UtcNow
                };
                await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);

                var payment = new Payment
                {
                    WalletId = wallet.WalletId,
                    TransactionId = walletTransaction.TransactionId,
                    AmountPaid = learningRegis.RemainingAmount.Value,
                    PaymentMethod = PaymentMethod.Wallet,
                    PaymentFor = PaymentFor.LearningRegistration,
                    Status = PaymentStatus.Completed
                };
                await _unitOfWork.PaymentsRepository.AddAsync(payment);

                learningRegis.Status = LearningRegis.Sixty;
                learningRegis.RemainingAmount = 0;
                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                var notifications = await _unitOfWork.StaffNotificationRepository
                    .GetQuery()
                    .Where(n => n.LearningRegisId == learningRegisId &&
                               n.Type == NotificationType.PaymentReminder &&
                               n.Status != NotificationStatus.Resolved)
                    .ToListAsync();

                if (notifications.Any())
                {
                    foreach (var notification in notifications)
                    {
                        notification.Status = NotificationStatus.Resolved;
                        await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                    }
                }

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Thanh toán số tiền còn lại thành công.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO { IsSucceed = false, Message = "Thanh toán thất bại. " + ex.Message };
            }
        }

        public async Task<ResponseDTO> RejectPaymentAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Không tìm thấy đăng ký học tập." };
                }

                string originalLearningRequest = learningRegis.LearningRequest;

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                bool is40PercentRejection = learningRegis.Status == LearningRegis.Accepted ||
                                    learningRegis.Status == LearningRegis.Fourty;

                bool is60PercentRejection = learningRegis.Status == LearningRegis.FourtyFeedbackDone ||
                                            learningRegis.Status == LearningRegis.Sixty;

                if (is40PercentRejection)
                {
                    learningRegis.Status = LearningRegis.Payment40Rejected;

                    var schedules = await _unitOfWork.ScheduleRepository.GetSchedulesByLearningRegisIdAsync(learningRegisId);
                    foreach (var schedule in schedules)
                    {
                        await _unitOfWork.ScheduleRepository.DeleteAsync(schedule.ScheduleId);
                    }

                    var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                        .GetByLearningRegisIdAsync(learningRegisId);
                    foreach (var session in learningPathSessions)
                    {
                        await _unitOfWork.LearningPathSessionRepository.DeleteAsync(session.LearningPathSessionId);
                    }

                    learningRegis.HasPendingLearningPath = false;
                }
                else if (is60PercentRejection)
                {
                    learningRegis.Status = LearningRegis.Payment60Rejected;

                    var schedules = await _unitOfWork.ScheduleRepository.GetSchedulesByLearningRegisIdAsync(learningRegisId);
                    foreach (var schedule in schedules)
                    {
                        await _unitOfWork.ScheduleRepository.DeleteAsync(schedule.ScheduleId);
                    }

                    var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                        .GetByLearningRegisIdAsync(learningRegisId);
                    foreach (var session in learningPathSessions)
                    {
                        await _unitOfWork.LearningPathSessionRepository.DeleteAsync(session.LearningPathSessionId);
                    }

                    learningRegis.HasPendingLearningPath = false;
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Việc từ chối thanh toán chỉ áp dụng cho các đăng ký học tập có trạng thái thanh toán 40% hoặc 60%."
                    };
                }

                var notifications = await _unitOfWork.StaffNotificationRepository
                    .GetQuery()
                    .Where(n => n.LearningRegisId == learningRegisId &&
                              n.Status != NotificationStatus.Resolved)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.Status = NotificationStatus.Resolved;
                    await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                }

                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = is40PercentRejection ?
                        "Thanh toán 40% đã bị từ chối và đăng ký học tập đã được đánh dấu là bị từ chối." :
                        "Thanh toán 60% đã bị từ chối và đăng ký học tập đã được đánh dấu là bị hủy.",
                    Data = new
                    {
                        LearningRegisId = learningRegisId,
                        Status = learningRegis.Status.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO { IsSucceed = false, Message = "Từ chối thanh toán thất bại: " + ex.Message };
            }
        }

        public async Task<ResponseDTO> GetClassInitialPaymentsAsync(int? classId)
        {
            try
            {
                var query = _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => p.Status == PaymentStatus.Completed &&
                               p.PaymentFor == PaymentFor.LearningRegistration)
                    .Include(p => p.Wallet)
                    .ThenInclude(w => w.Learner)
                    .Include(p => p.WalletTransaction);

                var payments = await query.ToListAsync();

                var registrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(lr => lr.ClassId != null)
                    .Include(lr => lr.Classes)
                    .Include(lr => lr.Learner)
                    .ToListAsync();

                var classRegistrations = registrations
                    .Where(lr => classId == null || lr.ClassId == classId)
                    .ToList();

                var walletTransactions = await _unitOfWork.WalletTransactionRepository
                    .GetQuery()
                    .Where(wt => wt.Status == TransactionStatus.Complete &&
                                 wt.TransactionType == TransactionType.Payment)
                    .Include(wt => wt.Wallet)
                    .ToListAsync();

                var registrationResults = new List<object>();

                foreach (var registration in classRegistrations)
                {
                    var classPrice = registration.Classes?.Price ?? 0;
                    var totalDays = registration.Classes?.totalDays ?? 0;
                    var totalClassPrice = classPrice * totalDays;
                    var expectedInitialPayment = Math.Round(totalClassPrice * 0.1m, 2);

                    var matchingPayments = payments
                        .Where(p => p.Wallet.LearnerId == registration.LearnerId)
                        .ToList();

                    var learnerWalletTransactions = walletTransactions
                        .Where(wt => wt.Wallet.LearnerId == registration.LearnerId &&
                                    wt.TransactionDate >= registration.RequestDate.AddDays(-3))
                        .OrderByDescending(wt => wt.TransactionDate)
                        .Take(5)
                        .ToList();

                    var fullMatches = matchingPayments
                        .Where(p => Math.Abs(p.AmountPaid - expectedInitialPayment) < 0.1m &&
                                    p.WalletTransaction?.TransactionDate >= registration.RequestDate)
                        .OrderBy(p => p.WalletTransaction.TransactionDate)
                        .ToList();

                    var payment = fullMatches.FirstOrDefault();

                    registrationResults.Add(new
                    {
                        LearnerId = registration.LearnerId,
                        LearnerName = registration.Learner?.FullName ?? "Unknown",
                        ClassId = registration.ClassId,
                        ClassName = registration.Classes?.ClassName ?? "Unknown",
                        LearningRegisId = registration.LearningRegisId,
                        AmountPaid = payment?.AmountPaid ?? 0,
                        PaymentDate = registration.RequestDate,
                        PaymentPercentage = "10%",
                        TotalClassPrice = totalClassPrice,
                        ExpectedPayment = expectedInitialPayment,
                        Status = registration.Status.ToString(),
                        RegistrationDate = registration.RequestDate,
                        WalletTransactions = learnerWalletTransactions.Select(wt => new {
                            TransactionId = wt.TransactionId,
                            Amount = wt.Amount,
                            Date = wt.TransactionDate,
                            TransactionType = wt.TransactionType.ToString(),
                            Status = wt.Status.ToString(),
                            PaymentRecord = payments.FirstOrDefault(p => p.TransactionId == wt.TransactionId) != null
                        }).ToList(),
                        Registration = new
                        {
                            Status = registration.Status.ToString(),
                            Price = registration.Price,
                            RequestDate = registration.RequestDate
                        }
                    });
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy thông tin đăng ký lớp học với chi tiết thanh toán thành công",
                    Data = registrationResults
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy đăng ký lớp học: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> ConfirmClassRemainingPaymentAsync(int learnerId, int classId)
        {
            try
            {
                var registration = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(lr => lr.LearnerId == learnerId &&
                                lr.ClassId == classId &&
                                lr.Status == LearningRegis.Accepted)
                    .Include(lr => lr.Classes)
                    .FirstOrDefaultAsync();

                if (registration == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy đăng ký học tập hoặc không ở trạng thái đã chấp nhận."
                    };
                }

                var wallet = await _unitOfWork.WalletRepository
                    .GetFirstOrDefaultAsync(w => w.LearnerId == learnerId);

                if (wallet == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy ví tiền của học viên này."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var classPrice = registration.Classes?.Price ?? 0;
                    var totalDays = registration.Classes?.totalDays ?? 0;
                    var totalClassPrice = classPrice * totalDays;
                    decimal remainingAmount = Math.Round(totalClassPrice * 0.9m, 0);

                    var walletTransaction = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        WalletId = wallet.WalletId,
                        Amount = remainingAmount,
                        TransactionType = TransactionType.Payment,
                        Status = TransactionStatus.Complete,
                        TransactionDate = DateTime.UtcNow,
                    };
                    await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);

                    var payment = new Payment
                    {
                        WalletId = wallet.WalletId,
                        TransactionId = walletTransaction.TransactionId,
                        AmountPaid = remainingAmount,
                        PaymentMethod = PaymentMethod.Offline,
                        PaymentFor = PaymentFor.ClassRegistration,
                        Status = PaymentStatus.Completed
                    };
                    await _unitOfWork.PaymentsRepository.AddAsync(payment);

                    registration.Status = LearningRegis.FullyPaid;
                    registration.RemainingAmount = 0;
                    await _unitOfWork.LearningRegisRepository.UpdateAsync(registration);

                    await _unitOfWork.SaveChangeAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Đã xác nhận thanh toán 90% còn lại của lớp học thành công.",
                        Data = new
                        {
                            LearnerId = learnerId,
                            ClassId = classId,
                            LearningRegisId = registration.LearningRegisId,
                            ClassName = registration.Classes?.ClassName,
                            AmountPaid = remainingAmount,
                            TransactionId = walletTransaction.TransactionId,
                            PaymentDate = walletTransaction.TransactionDate,
                            Status = "Đã Hoàn Thành Thanh Toán"
                        }
                    };
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Lỗi khi xác nhận thanh toán: {ex.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi xử lý xác nhận thanh toán: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetFullyPaidLearnersInClassAsync(int classId)
        {
            try
            {
                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
                if (classEntity == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy lớp học."
                    };
                }

                var registrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(lr => lr.ClassId == classId && lr.Status == LearningRegis.FullyPaid)
                    .Include(lr => lr.Learner)
                    .ToListAsync();

                if (!registrations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Không tìm thấy học viên đã thanh toán đầy đủ cho lớp học này.",
                        Data = new List<object>()
                    };
                }

                var learnerIds = registrations.Select(r => r.LearnerId).ToList();

                var wallets = await _unitOfWork.WalletRepository
                    .GetQuery()
                    .Where(w => learnerIds.Contains(w.LearnerId))
                    .ToListAsync();

                var walletIds = wallets.Select(w => w.WalletId).ToList();

                var payments = await _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => walletIds.Contains(p.WalletId) &&
                              p.Status == PaymentStatus.Completed &&
                              p.PaymentFor == PaymentFor.ClassRegistration)
                    .Include(p => p.WalletTransaction)
                    .ToListAsync();

                var walletByLearnerId = wallets.ToDictionary(w => w.LearnerId, w => w);

                var fullyPaidLearners = registrations.Select(reg => {
                    walletByLearnerId.TryGetValue(reg.LearnerId, out var wallet);

                    var learnerPayments = wallet != null
                        ? payments.Where(p => p.WalletId == wallet.WalletId).ToList()
                        : new List<Payment>();

                    decimal totalPaid = learnerPayments.Sum(p => p.AmountPaid);

                    var latestPayment = learnerPayments.OrderByDescending(p => p.WalletTransaction?.TransactionDate).FirstOrDefault();

                    return new
                    {
                        LearnerId = reg.LearnerId,
                        LearnerName = reg.Learner?.FullName ?? "Unknown",
                        LearningRegisId = reg.LearningRegisId,
                        Status = reg.Status.ToString(),
                        TotalAmountPaid = totalPaid,
                        LastPaymentDate = latestPayment?.WalletTransaction?.TransactionDate,
                        PaymentMethod = latestPayment?.PaymentMethod.ToString() ?? "Unknown",
                        RegistrationDate = reg.RequestDate
                    };
                }).ToList();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã tìm thấy {fullyPaidLearners.Count} học viên đã thanh toán đầy đủ cho lớp học ID: {classId}",
                    Data = fullyPaidLearners
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy danh sách học viên đã thanh toán đầy đủ: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetClassPaymentStatusAsync(int classId)
        {
            try
            {
                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
                if (classEntity == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy lớp học."
                    };
                }

                var registrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(lr => lr.ClassId == classId)
                    .Include(lr => lr.Learner)
                    .Include(lr => lr.Classes)
                    .ToListAsync();

                if (!registrations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Không tìm thấy học viên nào cho lớp học này.",
                        Data = new { PartiallyPaid = new List<object>(), FullyPaid = new List<object>() }
                    };
                }

                var learnerIds = registrations.Select(r => r.LearnerId).ToList();

                var wallets = await _unitOfWork.WalletRepository
                    .GetQuery()
                    .Where(w => learnerIds.Contains(w.LearnerId))
                    .ToListAsync();

                var walletIds = wallets.Select(w => w.WalletId).ToList();

                var payments = await _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => walletIds.Contains(p.WalletId) &&
                              p.Status == PaymentStatus.Completed &&
                              (p.PaymentFor == PaymentFor.ClassRegistration ||
                               p.PaymentFor == PaymentFor.LearningRegistration))
                    .Include(p => p.WalletTransaction)
                    .ToListAsync();

                var walletByLearnerId = wallets.ToDictionary(w => w.LearnerId, w => w);

                var fullyPaidLearners = registrations
                    .Where(reg => reg.Status == LearningRegis.FullyPaid)
                    .Select(reg => {

                        var classPrice = reg.Classes?.Price ?? 0;
                        var totalDays = reg.Classes?.totalDays ?? 0;
                        var totalClassPrice = classPrice * totalDays;
                        walletByLearnerId.TryGetValue(reg.LearnerId, out var wallet);

                        var learnerPayments = wallet != null
                            ? payments.Where(p => p.WalletId == wallet.WalletId &&
                                                 p.PaymentFor == PaymentFor.ClassRegistration).ToList()
                            : new List<Payment>();

                        decimal totalPaid = learnerPayments.Sum(p => p.AmountPaid);

                        var latestPayment = learnerPayments.OrderByDescending(p => p.WalletTransaction?.TransactionDate).FirstOrDefault();

                        return new
                        {
                            LearnerId = reg.LearnerId,
                            LearnerName = reg.Learner?.FullName ?? "Unknown",
                            LearningRegisId = reg.LearningRegisId,
                            Status = reg.Status.ToString(),
                            TotalClassPrice = totalClassPrice,
                            TotalAmountPaid = totalPaid,
                            LastPaymentDate = latestPayment?.WalletTransaction?.TransactionDate,
                            PaymentMethod = latestPayment?.PaymentMethod.ToString() ?? "Unknown",
                            RegistrationDate = reg.RequestDate,
                            IsFullyPaid = true,
                            IsEligible = true
                        };
                    }).ToList();

                var partiallyPaidLearners = registrations
                    .Where(reg => reg.Status == LearningRegis.Accepted && reg.Status != LearningRegis.FullyPaid)
                    .Select<Learning_Registration, object>(reg => {
                        var classPrice = reg.Classes?.Price ?? 0;
                        var totalDays = reg.Classes?.totalDays ?? 0;
                        var totalClassPrice = classPrice * totalDays;
                        var expectedInitialPayment = Math.Round(totalClassPrice * 0.1m, 2);

                        walletByLearnerId.TryGetValue(reg.LearnerId, out var wallet);

                        if (wallet == null)
                            return null;

                        var matchingPayments = payments.Where(p => p.WalletId == wallet.WalletId &&
                                                p.PaymentFor == PaymentFor.LearningRegistration &&
                                                Math.Abs(p.AmountPaid - expectedInitialPayment) < 0.1m &&
                                                p.WalletTransaction?.TransactionDate >= reg.RequestDate)
                                            .ToList();

                        var payment = matchingPayments.OrderBy(p => p.WalletTransaction?.TransactionDate).FirstOrDefault();

                        if (payment == null)
                        {
                            var walletTxs = _unitOfWork.WalletTransactionRepository
                                .GetQuery()
                                .Where(wt => wt.WalletId == wallet.WalletId &&
                                        wt.Status == TransactionStatus.Complete &&
                                        wt.TransactionType == TransactionType.Payment &&
                                        Math.Abs(wt.Amount - expectedInitialPayment) < 0.1m &&
                                        wt.TransactionDate >= reg.RequestDate.AddSeconds(-1))
                                .OrderBy(wt => wt.TransactionDate)
                                .ToListAsync().Result;

                            if (!walletTxs.Any())
                                return null;

                            return new
                            {
                                LearnerId = reg.LearnerId,
                                LearnerName = reg.Learner?.FullName ?? "Unknown",
                                LearningRegisId = reg.LearningRegisId,
                                ClassId = reg.ClassId,
                                ClassName = reg.Classes?.ClassName ?? "Unknown",
                                AmountPaid = walletTxs.First().Amount,
                                PaymentDate = walletTxs.First().TransactionDate,
                                PaymentPercentage = "10%",
                                TotalClassPrice = totalClassPrice,
                                ExpectedPayment = expectedInitialPayment,
                                Status = reg.Status.ToString(),
                                RegistrationDate = reg.RequestDate,
                                IsFullyPaid = false,
                                RemainingAmount = totalClassPrice * 0.9m,
                                PaymentSource = "Wallet Transaction",
                                IsEligible = false
                            };
                        }

                        return new
                        {
                            LearnerId = reg.LearnerId,
                            LearnerName = reg.Learner?.FullName ?? "Unknown",
                            LearningRegisId = reg.LearningRegisId,
                            ClassId = reg.ClassId,
                            ClassName = reg.Classes?.ClassName ?? "Unknown",
                            AmountPaid = payment.AmountPaid,
                            PaymentDate = payment.WalletTransaction?.TransactionDate,
                            PaymentPercentage = "10%",
                            TotalClassPrice = totalClassPrice,
                            ExpectedPayment = expectedInitialPayment,
                            Status = reg.Status.ToString(),
                            RegistrationDate = reg.RequestDate,
                            IsFullyPaid = false,
                            RemainingAmount = totalClassPrice * 0.9m,
                            PaymentSource = "Payment Record"
                        };
                    })
                    .Where(item => item != null)
                    .ToList();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã lấy trạng thái thanh toán cho lớp học ID: {classId} thành công",
                    Data = new
                    {
                        ClassId = classId,
                        ClassName = classEntity.ClassName,
                        TotalRegistrations = registrations.Count,
                        PartiallyPaidCount = partiallyPaidLearners.Count,
                        FullyPaidCount = fullyPaidLearners.Count,
                        PartiallyPaidLearners = partiallyPaidLearners,
                        FullyPaidLearners = fullyPaidLearners
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy trạng thái thanh toán của lớp học: {ex.Message}"
                };
            }
        }
    }
}