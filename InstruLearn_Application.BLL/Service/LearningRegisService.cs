using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Enum;
using System.Transactions;
using Microsoft.Extensions.Logging;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using InstruLearn_Application.Model.Models.DTO.LearnerClass;
using Microsoft.EntityFrameworkCore;
using InstruLearn_Application.Model.Models.DTO.Certification;
using Microsoft.Extensions.DependencyInjection;
using System.Dynamic;
using System.Text.Json;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningRegisService : ILearningRegisService
    {
        private readonly ILearningRegisRepository _learningRegisRepository;
        private readonly ILogger<LearningRegisService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IScheduleService _scheduleService;
        private readonly IEmailService _emailService;
        private readonly IServiceProvider _serviceProvider;

        public LearningRegisService(ILearningRegisRepository learningRegisRepository, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LearningRegisService> logger, IScheduleService scheduleService, IEmailService emailService, IServiceProvider serviceProvider)
        {
            _learningRegisRepository = learningRegisRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _scheduleService = scheduleService;
            _emailService = emailService;
            _serviceProvider = serviceProvider;
        }
        public async Task<ResponseDTO> GetAllLearningRegisAsync()
        {
            try
            {
                var allRegistrations = await _learningRegisRepository.GetAllAsync();
                var allDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(allRegistrations).ToList();

                var enrichedRegistrations = new List<object>();
                foreach (var regDto in allDtos)
                {
                    var registrationDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                        JsonSerializer.Serialize(regDto));

                    var firstPaymentPeriod = await GetFirstPaymentPeriodInfoAsync(regDto.LearningRegisId);
                    var secondPaymentPeriod = await GetSecondPaymentPeriodInfoAsync(regDto.LearningRegisId);

                    var enrichedReg = new Dictionary<string, object>();
                    foreach (var kvp in registrationDict)
                    {
                        enrichedReg[kvp.Key] = JsonSerializer.Deserialize<object>(kvp.Value.GetRawText());
                    }

                    enrichedReg["firstPaymentPeriod"] = firstPaymentPeriod;
                    enrichedReg["secondPaymentPeriod"] = secondPaymentPeriod;

                    var registration = allRegistrations.FirstOrDefault(lr => lr.LearningRegisId == regDto.LearningRegisId);

                    if (registration != null)
                    {
                        var availableDayValues = new List<DayOfWeek>();
                        if (registration.LearningRegistrationDay != null && registration.LearningRegistrationDay.Any())
                        {
                            foreach (var day in registration.LearningRegistrationDay)
                            {
                                string dayString = day.DayOfWeek.ToString();

                                if (Enum.TryParse<DayOfWeek>(dayString, true, out var dayOfWeek))
                                {
                                    availableDayValues.Add(dayOfWeek);
                                }
                            }
                        }

                        if (registration.Schedules != null && registration.Schedules.Any())
                        {
                            _logger.LogInformation($"Processing schedules for registration ID: {registration.LearningRegisId}. Found {registration.Schedules.Count} total schedule(s)");

                            var orderedSchedules = registration.Schedules
                                .OrderBy(s => s.StartDay)
                                .ThenBy(s => s.TimeStart)
                                .ToList();

                            regDto.SessionDates = orderedSchedules
                                .Select(s => $"{s.StartDay:yyyy-MM-dd} {s.TimeStart:HH:mm}")
                                .ToList();
                        }
                        else if (registration.StartDay.HasValue &&
                                 availableDayValues.Count > 0 &&
                                 registration.NumberOfSession > 0)
                        {
                            _logger.LogInformation($"No schedules found for registration ID: {registration.LearningRegisId}. Generating dates based on learning days.");

                            DateOnly currentDate = registration.StartDay.Value;

                            var sessionDates = new List<string>();
                            int sessionsFound = 0;
                            int maxAttempts = 100;

                            while (sessionsFound < registration.NumberOfSession && sessionsFound < maxAttempts)
                            {
                                if (availableDayValues.Contains(currentDate.DayOfWeek))
                                {
                                    sessionDates.Add($"{currentDate:yyyy-MM-dd} {registration.TimeStart:HH:mm}");
                                    sessionsFound++;
                                }

                                currentDate = currentDate.AddDays(1);
                            }

                            regDto.SessionDates = sessionDates;
                        }
                        else
                        {
                            _logger.LogWarning($"Unable to calculate session dates for registration ID: {registration.LearningRegisId}");
                            regDto.SessionDates = new List<string>();
                        }

                        enrichedReg["SessionDates"] = regDto.SessionDates;
                    }

                    enrichedRegistrations.Add(enrichedReg);
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "All learning registrations retrieved successfully.",
                    Data = enrichedRegistrations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving learning registrations with payment information");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving learning registrations: {ex.Message}"
                };
            }
        }


        public async Task<ResponseDTO> GetLearningRegisByIdAsync(int learningRegisId)
        {
            try
            {
                var registration = await _learningRegisRepository.GetByIdAsync(learningRegisId);
                if (registration == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning registration not found.",
                        Data = null
                    };
                }

                var dto = _mapper.Map<OneOnOneRegisDTO>(registration);

                var firstPaymentPeriod = await GetFirstPaymentPeriodInfoAsync(learningRegisId);
                var secondPaymentPeriod = await GetSecondPaymentPeriodInfoAsync(learningRegisId);

                dynamic enrichedDto = new ExpandoObject();
                var enrichedDict = (IDictionary<string, object>)enrichedDto;

                var dtoProps = typeof(OneOnOneRegisDTO).GetProperties();
                foreach (var prop in dtoProps)
                {
                    enrichedDict[prop.Name] = prop.GetValue(dto);
                }

                enrichedDict["firstPaymentPeriod"] = firstPaymentPeriod;
                enrichedDict["secondPaymentPeriod"] = secondPaymentPeriod;

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Learning registration retrieved successfully.",
                    Data = enrichedDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving learning registration {learningRegisId} with payment information");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving learning registration: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO> GetRegistrationsByTeacherIdAsync(int teacherId)
        {
            try
            {
                var allRegistrations = await _learningRegisRepository.GetRegistrationsByTeacherIdAsync(teacherId);

                var filteredRegistrations = allRegistrations.Where(r =>
                    r.RegisTypeId == 1 &&
                    r.Status == LearningRegis.Accepted || r.Status == LearningRegis.Fourty
                ).ToList();

                var registrationDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(filteredRegistrations);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Filtered registrations for Teacher ID {teacherId} retrieved successfully.",
                    Data = registrationDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving registrations for teacher {teacherId}: {ex.Message}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving registrations: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateLearningRegisAsync(CreateLearningRegisDTO createLearningRegisDTO)
        {
            try
            {
                _logger.LogInformation("Starting learning registration process.");

                var scheduleConflict = await _scheduleService.CheckLearnerScheduleConflictAsync(createLearningRegisDTO.LearnerId, createLearningRegisDTO.StartDay.Value, createLearningRegisDTO.TimeStart, createLearningRegisDTO.TimeLearning);

                if (!scheduleConflict.IsSucceed)
                {
                    return scheduleConflict;
                }

                var existingRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(r =>
                        r.LearnerId == createLearningRegisDTO.LearnerId &&
                        //r.MajorId == createLearningRegisDTO.MajorId && 
                        r.TimeStart == createLearningRegisDTO.TimeStart &&
                        //r.TimeLearning == createLearningRegisDTO.TimeLearning &&
                        r.Status == LearningRegis.Pending)
                    .ToListAsync();

                if (existingRegistrations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "You already have a pending registration for this major. Please wait for it to be processed before creating a new one."
                    };
                }

                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == createLearningRegisDTO.LearnerId);

                    if (wallet == null)
                    {
                        _logger.LogWarning($"Wallet not found for learnerId: {createLearningRegisDTO.LearnerId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Wallet not found for the learner."
                        };
                    }

                    _logger.LogInformation($"Wallet found for learnerId: {createLearningRegisDTO.LearnerId}, balance: {wallet.Balance}");

                    if (wallet.Balance < 50000)
                    {
                        _logger.LogWarning($"Insufficient balance for learnerId: {createLearningRegisDTO.LearnerId}. Current balance: {wallet.Balance}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Insufficient balance in the wallet."
                        };
                    }

                    wallet.Balance -= 50000;
                    await _unitOfWork.WalletRepository.UpdateAsync(wallet);

                    if (createLearningRegisDTO.TimeLearning != 45 && createLearningRegisDTO.TimeLearning != 60 && createLearningRegisDTO.TimeLearning != 90 && createLearningRegisDTO.TimeLearning != 120)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Invalid learning duration. Please select 45, 60, 90 or 120 minutes."
                        };
                    }

                    var learningRegis = _mapper.Map<Learning_Registration>(createLearningRegisDTO);
                    learningRegis.Status = LearningRegis.Pending;

                    if (!string.IsNullOrEmpty(createLearningRegisDTO.SelfAssessment))
                    {
                        learningRegis.SelfAssessment = createLearningRegisDTO.SelfAssessment;
                    }

                    await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    if (createLearningRegisDTO.LearningDays != null && createLearningRegisDTO.LearningDays.Any())
                    {
                        var learningDays = createLearningRegisDTO.LearningDays.Select(day => new LearningRegistrationDay
                        {
                            LearningRegisId = learningRegis.LearningRegisId,
                            DayOfWeek = day
                        }).ToList();

                        await _unitOfWork.LearningRegisDayRepository.AddRangeAsync(learningDays);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    await _unitOfWork.SaveChangeAsync();

                    var walletTransaction = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        WalletId = wallet.WalletId,
                        Amount = 50000,
                        TransactionType = TransactionType.Payment,
                        Status = Model.Enum.TransactionStatus.Complete,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);
                    await _unitOfWork.SaveChangeAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation("Learning registration added successfully. Wallet balance updated.");

                    var timeEnd = learningRegis.TimeStart.AddMinutes(createLearningRegisDTO.TimeLearning);
                    var timeEndFormatted = timeEnd.ToString("HH:mm");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Learning Registration added successfully. Wallet balance updated. Status set to Pending.",
                        Data = new
                        {
                            LearningRegisId = learningRegis.LearningRegisId,
                            HasSelfAssessment = !string.IsNullOrEmpty(createLearningRegisDTO.SelfAssessment)
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing learning registration.");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


        public async Task<ResponseDTO> DeleteLearningRegisAsync(int learningRegisId)
        {
            var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
            if (learningRegis == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning Registration not found."
                };
            }
            await _unitOfWork.LearningRegisRepository.DeleteAsync(learningRegisId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration deleted successfully."
            };
        }

        public async Task<ResponseDTO> GetAllPendingRegistrationsAsync()
        {
            var pendingRegistrations = await _learningRegisRepository.GetPendingRegistrationsAsync();
            var pendingDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(pendingRegistrations);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Pending learning registrations retrieved successfully.",
                Data = pendingDtos
            };
        }

        public async Task<ResponseDTO> GetRegistrationsByLearnerIdAsync(int learnerId)
        {
            try
            {
                var registrations = await _learningRegisRepository.GetRegistrationsByLearnerIdAsync(learnerId);
                var registrationDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(registrations).ToList();

                var enrichedRegistrations = new List<object>();
                foreach (var regDto in registrationDtos)
                {
                    var registrationDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                        JsonSerializer.Serialize(regDto));

                    var firstPaymentPeriod = await GetFirstPaymentPeriodInfoAsync(regDto.LearningRegisId);
                    var secondPaymentPeriod = await GetSecondPaymentPeriodInfoAsync(regDto.LearningRegisId);

                    var enrichedReg = new Dictionary<string, object>();
                    foreach (var kvp in registrationDict)
                    {
                        enrichedReg[kvp.Key] = JsonSerializer.Deserialize<object>(kvp.Value.GetRawText());
                    }

                    enrichedReg["firstPaymentPeriod"] = firstPaymentPeriod;
                    enrichedReg["secondPaymentPeriod"] = secondPaymentPeriod;

                    enrichedRegistrations.Add(enrichedReg);
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"All registrations for Learner ID {learnerId} retrieved successfully.",
                    Data = enrichedRegistrations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving learning registrations for learner {learnerId} with payment information");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving learning registrations: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateLearningRegisStatusAsync(UpdateLearningRegisDTO updateDTO)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(updateDTO.LearningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found."
                    };
                }

                var levelAssigned = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(updateDTO.LevelId);
                if (levelAssigned == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Level Assigned not found."
                    };
                }

                int? originalTeacherId = learningRegis.TeacherId;

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                _mapper.Map(updateDTO, learningRegis);

                learningRegis.Price = levelAssigned.LevelPrice * learningRegis.NumberOfSession;

                learningRegis.Status = LearningRegis.Accepted;
                learningRegis.AcceptedDate = DateTime.Now;

                learningRegis.LearningPath = levelAssigned.SyllabusLink;

                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                await _unitOfWork.SaveChangeAsync();

                decimal totalPrice = learningRegis.Price.Value;

                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learningRegis.LearnerId);
                var account = learner != null ? await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId) : null;

                bool teacherChanged = originalTeacherId != learningRegis.TeacherId;

                Teacher originalTeacher = null;
                if (teacherChanged && originalTeacherId.HasValue)
                {
                    originalTeacher = await _unitOfWork.TeacherRepository.GetByIdAsync(originalTeacherId.Value);
                }

                var currentTeacher = learningRegis.TeacherId.HasValue
                    ? await _unitOfWork.TeacherRepository.GetByIdAsync(learningRegis.TeacherId.Value)
                    : null;

                if (currentTeacher != null)
                {
                    var teacherNotification = new StaffNotification
                    {
                        Title = "Yêu cầu tạo lộ trình học tập",
                        Message = $"Đơn đăng ký học mã số #{learningRegis.LearningRegisId} cho học viên {learner?.FullName ?? "không xác định"} " +
                                 $"đã được phê duyệt. Vui lòng tạo lộ trình cho học viên này.",
                        LearningRegisId = learningRegis.LearningRegisId,
                        LearnerId = learningRegis.LearnerId,
                        CreatedAt = DateTime.Now,
                        Status = NotificationStatus.Unread,
                        Type = NotificationType.CreateLearningPath
                    };

                    await _unitOfWork.StaffNotificationRepository.AddAsync(teacherNotification);
                    await _unitOfWork.SaveChangeAsync();
                }

                if (account != null && !string.IsNullOrEmpty(account.Email))
                {
                    try
                    {
                        string subject = "Yêu cầu đăng ký học của bạn đã được phê duyệt";

                        string teacherChangeNotice = "";
                        if (teacherChanged)
                        {
                            string originalTeacherName = originalTeacher?.Fullname ?? "Không có giáo viên";
                            string currentTeacherName = currentTeacher?.Fullname ?? "Không có giáo viên";

                            teacherChangeNotice = $@"
                    <div style='background-color: #fff3cd; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ffc107;'>
                        <h3 style='margin-top: 0; color: #856404;'>Thông báo thay đổi giáo viên</h3>
                        <p>Giáo viên của bạn đã được thay đổi từ <strong>{originalTeacherName}</strong> sang <strong>{currentTeacherName}</strong>.</p>
                    </div>";
                        }

                        string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                        <h2 style='color: #333;'>Xin chào {learner.FullName},</h2>
                        
                        <p>Chúng tôi vui mừng thông báo rằng yêu cầu đăng ký học tập của bạn đã được chấp nhận.</p>
                        
                        {teacherChangeNotice}
                        
                        <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4CAF50;'>
                            <h3 style='margin-top: 0; color: #333;'>Thông tin đăng ký:</h3>
                            <p><strong>Giáo viên:</strong> {(currentTeacher != null ? currentTeacher.Fullname : "Chưa phân công")}</p>
                            <p><strong>Môn học:</strong> {learningRegis.Major?.MajorName ?? "N/A"}</p>
                            <p><strong>Tổng số buổi học:</strong> {learningRegis.NumberOfSession}</p>
                            <p><strong>Tổng học phí:</strong> {totalPrice:N0} VND</p>
                        </div>
                        
                        <p>Giáo viên của bạn sẽ chuẩn bị một lộ trình học tập cho bạn. Bạn sẽ nhận được một thông báo khác khi lộ trình học tập của bạn sẵn sàng, bao gồm cả thông tin về thời hạn thanh toán.</p>
                        
                        <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                            <a href='https://instrulearn.com/learning-registrations/{learningRegis.LearningRegisId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                                Xem Chi Tiết Đăng Ký
                            </a>
                        </div>
                        
                        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.</p>
                        
                        <p>Trân trọng,<br>Nhóm InstruLearn</p>
                    </div>
                </body>
                </html>";

                        await _emailService.SendEmailAsync(
                            account.Email,
                            subject,
                            body,
                            isHtml: true
                        );

                        _logger.LogInformation($"Thông báo về việc phê duyệt đăng ký học đã được gửi đến địa chỉ email {account.Email} cho đơn đăng ký mã số {learningRegis.LearningRegisId}");
                        if (teacherChanged)
                        {
                            _logger.LogInformation($"Thông báo về việc thay đổi giáo viên đã được gửi đến địa chỉ email {account.Email} cho đơn đăng ký học mã số {learningRegis.LearningRegisId}. Giáo viên đã được thay đổi từ mã {originalTeacherId} sang {learningRegis.TeacherId}");
                        }
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Gửi email thông báo phê duyệt đăng ký học không thành công");
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Learning Registration updated successfully with total price {totalPrice:F2} VND. Notification email sent to learner.",
                    Data = new
                    {
                        LearningRegisId = learningRegis.LearningRegisId,
                        TotalPrice = totalPrice,
                        SyllabusLink = levelAssigned.SyllabusLink,
                        EmailSent = account != null && !string.IsNullOrEmpty(account.Email),
                        TeacherChanged = teacherChanged
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to update Learning Registration. " + ex.Message
                };
            }
        }

        public async Task<ResponseDTO> JoinClassWithWalletPaymentAsync(LearnerClassPaymentDTO paymentDTO)
        {
            try
            {
                _logger.LogInformation($"Starting class enrollment process for learner ID: {paymentDTO.LearnerId}, class ID: {paymentDTO.ClassId}");
                var classScheduleConflict = await _scheduleService.CheckLearnerClassScheduleConflictAsync(paymentDTO.LearnerId, paymentDTO.ClassId);

                if (!classScheduleConflict.IsSucceed)
                {
                    return classScheduleConflict;
                }

                var existingEnrollments = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .AnyAsync(lr =>
                        lr.LearnerId == paymentDTO.LearnerId &&
                        lr.ClassId == paymentDTO.ClassId &&
                        (lr.Status == LearningRegis.Pending || lr.Status == LearningRegis.Accepted || lr.Status == LearningRegis.Fourty || lr.Status == LearningRegis.Sixty));

                if (existingEnrollments)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "You already have an enrollment or pending enrollment for this class."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(paymentDTO.ClassId);
                    if (classEntity == null)
                    {
                        _logger.LogWarning($"Class with ID {paymentDTO.ClassId} not found");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Class with ID {paymentDTO.ClassId} not found."
                        };
                    }

                    int? levelId = classEntity.LevelId;
                    if (!levelId.HasValue)
                    {
                        _logger.LogWarning($"Class with ID {paymentDTO.ClassId} doesn't have an associated level");
                    }
                    else
                    {
                        _logger.LogInformation($"Using level ID {levelId} from class {paymentDTO.ClassId}");
                    }

                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(paymentDTO.LearnerId);
                    if (learner == null)
                    {
                        _logger.LogWarning($"Learner with ID {paymentDTO.LearnerId} not found");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Learner with ID {paymentDTO.LearnerId} not found."
                        };
                    }

                    var existingEnrollment = await _unitOfWork.dbContext.Learner_Classes
                        .FirstOrDefaultAsync(lc => lc.LearnerId == paymentDTO.LearnerId && lc.ClassId == paymentDTO.ClassId);

                    if (existingEnrollment != null)
                    {
                        _logger.LogWarning($"Learner {paymentDTO.LearnerId} is already enrolled in class {paymentDTO.ClassId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "You are already enrolled in this class."
                        };
                    }

                    decimal pricePerDay = classEntity.Price;
                    if (pricePerDay <= 0)
                    {
                        _logger.LogWarning($"Invalid price for class {paymentDTO.ClassId}: {pricePerDay}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Invalid class price."
                        };
                    }

                    decimal totalClassPrice = pricePerDay * classEntity.totalDays;

                    decimal paymentAmount = totalClassPrice * 0.1m;

                    paymentAmount = Math.Round(paymentAmount, 2);

                    _logger.LogInformation($"Class price calculation: {pricePerDay} per day × {classEntity.totalDays} days = {totalClassPrice} total. 10% payment: {paymentAmount}");

                    var classRegisType = await _unitOfWork.LearningRegisTypeRepository.GetQuery()
                        .FirstOrDefaultAsync(rt => rt.RegisTypeName.Contains("Center"));

                    if (classRegisType == null)
                    {
                        _logger.LogWarning("Class registration type not found in the database");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Class registration type not found in the system."
                        };
                    }

                    var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == paymentDTO.LearnerId);
                    if (wallet == null)
                    {
                        _logger.LogWarning($"Wallet not found for learner {paymentDTO.LearnerId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Wallet not found for your account."
                        };
                    }

                    if (wallet.Balance < paymentAmount)
                    {
                        _logger.LogWarning($"Insufficient balance for learner {paymentDTO.LearnerId}. Required: {paymentAmount}, Available: {wallet.Balance}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Insufficient balance. Required: {paymentAmount} (10% of total {totalClassPrice}), Available: {wallet.Balance}"
                        };
                    }

                    wallet.Balance -= paymentAmount;
                    var walletUpdateResult = await _unitOfWork.WalletRepository.UpdateAsync(wallet);
                    if (!walletUpdateResult)
                    {
                        throw new Exception("Failed to update wallet balance");
                    }
                    await _unitOfWork.SaveChangeAsync();

                    var walletTransaction = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        WalletId = wallet.WalletId,
                        Amount = paymentAmount,
                        TransactionType = TransactionType.Payment,
                        Status = Model.Enum.TransactionStatus.Complete,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);
                    await _unitOfWork.SaveChangeAsync();

                    var learningRegis = new Learning_Registration
                    {
                        LearnerId = paymentDTO.LearnerId,
                        ClassId = paymentDTO.ClassId,
                        TeacherId = classEntity.TeacherId,
                        RegisTypeId = classRegisType.RegisTypeId,
                        MajorId = classEntity.MajorId,
                        LevelId = levelId,
                        Status = LearningRegis.Accepted,
                        RequestDate = DateTime.UtcNow,
                        Price = totalClassPrice,
                        NumberOfSession = classEntity.totalDays,
                        TimeStart = classEntity.ClassTime,
                        TimeLearning = 120,
                        StartDay = classEntity.StartDate,
                        VideoUrl = string.Empty,
                        LearningRequest = string.Empty
                    };

                    await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    var learnerClass = new Learner_class
                    {
                        LearnerId = paymentDTO.LearnerId,
                        ClassId = paymentDTO.ClassId
                    };

                    await _unitOfWork.dbContext.Learner_Classes.AddAsync(learnerClass);
                    await _unitOfWork.SaveChangeAsync();


                    DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                    if (classEntity.StartDate <= today)
                    {
                        _logger.LogInformation($"Class has already started. Creating certificate immediately for learner {paymentDTO.LearnerId} in class {paymentDTO.ClassId}");

                        try
                        {
                            string teacherName = "Unknown Teacher";
                            if (classEntity.Teacher != null)
                            {
                                teacherName = classEntity.Teacher.Fullname;
                            }
                            else
                            {
                                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(classEntity.TeacherId);
                                if (teacher != null)
                                {
                                    teacherName = teacher.Fullname;
                                }
                            }

                            string majorName = "Unknown Subject";
                            if (classEntity.Major != null)
                            {
                                majorName = classEntity.Major.MajorName;
                            }
                            else
                            {
                                var major = await _unitOfWork.MajorRepository.GetByIdAsync(classEntity.MajorId);
                                if (major != null)
                                {
                                    majorName = major.MajorName;
                                }
                            }

                            var createCertificationDTO = new CreateCertificationDTO
                            {
                                LearnerId = paymentDTO.LearnerId,
                                ClassId = paymentDTO.ClassId,
                                CertificationType = CertificationType.CenterLearning,
                                CertificationName = $"Center Learning Certificate - {classEntity.ClassName}",
                                TeacherName = teacherName,
                                Subject = majorName
                            };

                            var certificationService = _serviceProvider.GetRequiredService<ICertificationService>();
                            var certResult = await certificationService.CreateCertificationAsync(createCertificationDTO);

                            if (certResult.IsSucceed)
                            {
                                _logger.LogInformation($"Certificate created successfully for learner {paymentDTO.LearnerId} in class {paymentDTO.ClassId}");
                            }
                            else
                            {
                                _logger.LogWarning($"Failed to create certificate: {certResult.Message}");
                            }
                        }
                        catch (Exception certEx)
                        {
                            _logger.LogError(certEx, $"Error creating certificate for learner {paymentDTO.LearnerId} in class {paymentDTO.ClassId}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Class starts on {classEntity.StartDate}. Creating notification for future certificate creation");

                        var staffNotification = new StaffNotification
                        {
                            Title = "Certificate Creation Scheduled",
                            Message = $"Create certificate for learner {learner.FullName} (ID: {paymentDTO.LearnerId}) in class {classEntity.ClassName} (ID: {paymentDTO.ClassId}) on start date {classEntity.StartDate}",
                            LearnerId = paymentDTO.LearnerId,
                            CreatedAt = DateTime.Now,
                            Status = NotificationStatus.Unread,
                            Type = NotificationType.Certificate
                        };

                        await _unitOfWork.StaffNotificationRepository.AddAsync(staffNotification);
                        await _unitOfWork.SaveChangeAsync();

                        _logger.LogInformation($"Certificate creation scheduled for class start date: {classEntity.StartDate}");
                    }

                    var existingLearnerSchedules = await _unitOfWork.ScheduleRepository.GetQuery()
                        .Where(s => s.ClassId == paymentDTO.ClassId &&
                                   s.TeacherId == classEntity.TeacherId &&
                                   s.LearnerId != null)
                        .OrderBy(s => s.StartDay)
                        .ToListAsync();

                    if (existingLearnerSchedules != null && existingLearnerSchedules.Any())
                    {
                        _logger.LogInformation($"Found {existingLearnerSchedules.Count} existing schedules for other learners in class {paymentDTO.ClassId}");

                        var uniqueDates = existingLearnerSchedules
                            .GroupBy(s => s.StartDay)
                            .Select(g => g.First())
                            .OrderBy(s => s.StartDay)
                            .Take(classEntity.totalDays)
                            .ToList();

                        var newSchedules = new List<Schedules>();

                        foreach (var existingSchedule in uniqueDates)
                        {
                            var newSchedule = new Schedules
                            {
                                LearnerId = paymentDTO.LearnerId,
                                ClassId = paymentDTO.ClassId,
                                LearningRegisId = learningRegis.LearningRegisId,
                                TeacherId = classEntity.TeacherId,
                                StartDay = existingSchedule.StartDay,
                                TimeStart = classEntity.ClassTime,
                                TimeEnd = classEntity.ClassTime.AddHours(2),
                                Mode = ScheduleMode.Center
                            };

                            newSchedules.Add(newSchedule);
                        }

                        await _unitOfWork.ScheduleRepository.AddRangeAsync(newSchedules);
                        await _unitOfWork.SaveChangeAsync();
                    }
                    else
                    {
                        var existingTeacherSchedules = await _unitOfWork.ScheduleRepository.GetQuery()
                            .Where(s => s.ClassId == paymentDTO.ClassId &&
                                       s.TeacherId == classEntity.TeacherId &&
                                       s.LearnerId == null)
                            .OrderBy(s => s.StartDay)
                            .ToListAsync();

                        if (existingTeacherSchedules != null && existingTeacherSchedules.Any())
                        {
                            _logger.LogInformation($"Found {existingTeacherSchedules.Count} existing teacher schedules for class {paymentDTO.ClassId}");

                            int schedulesUsed = 0;
                            var learnerSchedules = new List<Schedules>();

                            foreach (var teacherSchedule in existingTeacherSchedules.OrderBy(s => s.StartDay))
                            {
                                if (schedulesUsed >= classEntity.totalDays)
                                    break;

                                var learnerSchedule = new Schedules
                                {
                                    LearnerId = paymentDTO.LearnerId,
                                    ClassId = paymentDTO.ClassId,
                                    LearningRegisId = learningRegis.LearningRegisId,
                                    TeacherId = classEntity.TeacherId,
                                    StartDay = teacherSchedule.StartDay,
                                    TimeStart = classEntity.ClassTime,
                                    TimeEnd = classEntity.ClassTime.AddHours(2),
                                    Mode = ScheduleMode.Center
                                };

                                learnerSchedules.Add(learnerSchedule);
                                schedulesUsed++;
                            }

                            await _unitOfWork.ScheduleRepository.AddRangeAsync(learnerSchedules);
                            await _unitOfWork.SaveChangeAsync();
                        }
                        else
                        {
                            _logger.LogWarning($"No existing schedules found for class {paymentDTO.ClassId}, creating new ones");

                            var classDays = await _unitOfWork.ClassDayRepository.GetQuery()
                                .Where(cd => cd.ClassId == paymentDTO.ClassId)
                                .OrderBy(cd => cd.Day)
                                .ToListAsync();

                            if (classDays.Any())
                            {
                                var learnerSchedules = new List<Schedules>();
                                var startDay = classEntity.StartDate;
                                int schedulesCreated = 0;
                                int weekMultiplier = 0;

                                var scheduleDays = new List<DateOnly>();

                                while (scheduleDays.Count < classEntity.totalDays)
                                {
                                    foreach (var classDay in classDays.OrderBy(cd => cd.Day))
                                    {
                                        if (scheduleDays.Count >= classEntity.totalDays)
                                            break;

                                        var scheduleDay = GetDateForDayOfWeek(startDay, classDay.Day, weekMultiplier);
                                        scheduleDays.Add(scheduleDay);
                                    }

                                    weekMultiplier++;
                                }

                                foreach (var scheduleDay in scheduleDays.OrderBy(d => d))
                                {
                                    var learnerSchedule = new Schedules
                                    {
                                        LearnerId = paymentDTO.LearnerId,
                                        ClassId = paymentDTO.ClassId,
                                        LearningRegisId = learningRegis.LearningRegisId,
                                        TeacherId = classEntity.TeacherId,
                                        StartDay = scheduleDay,
                                        TimeStart = classEntity.ClassTime,
                                        TimeEnd = classEntity.ClassTime.AddHours(2),
                                        Mode = ScheduleMode.Center
                                    };

                                    learnerSchedules.Add(learnerSchedule);
                                    schedulesCreated++;

                                    if (schedulesCreated >= classEntity.totalDays)
                                        break;
                                }

                                await _unitOfWork.ScheduleRepository.AddRangeAsync(learnerSchedules);
                                await _unitOfWork.SaveChangeAsync();
                            }
                            else
                            {
                                _logger.LogWarning($"No class days found for class {paymentDTO.ClassId}. Enrollment may be incomplete.");
                            }
                        }
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    try
                    {
                        string formattedClassTime = classEntity.ClassTime.ToString("HH:mm");

                        string formattedTestDay = classEntity.TestDay.ToString("dd/MM/yyyy");

                        string notificationMessage = $@"Chào bạn,

Cảm ơn bạn đã đăng ký tham gia lớp học {classEntity.ClassName} tại InstruLearn.
Để hoàn tất việc xếp lớp, bạn vui lòng đến trung tâm InstruLearn vào lúc {formattedClassTime} ngày {formattedTestDay} để thực hiện kiểm tra chất lượng đầu vào.

Việc kiểm tra này giúp chúng tôi sắp xếp lớp phù hợp với trình độ hiện tại của bạn và thực hiện thanh toán.

Lưu ý:
Học viên vui lòng bỏ qua thông báo này nếu:
- Học viên đã thực hiện kiểm tra chất lượng đầu vào.
- Đã được chuyển lớp do chưa phù hợp với trình độ lớp đã đăng ký.
- Đã được chuyển lớp để phù hợp hơn với năng lực hiện tại.

Địa chỉ: 935 Huỳnh Tấn Phát, Phú Thuận, Quận 7, TP.HCM

Trân trọng,
InstruLearn";

                        var entranceTestNotification = new StaffNotification
                        {
                            LearnerId = paymentDTO.LearnerId,
                            Title = "Thông báo kiểm tra đầu vào lớp " + classEntity.ClassName,
                            Message = notificationMessage,
                            Type = NotificationType.EntranceTest,
                            Status = NotificationStatus.Unread,
                            CreatedAt = DateTime.Now
                        };

                        await _unitOfWork.StaffNotificationRepository.AddAsync(entranceTestNotification);
                        await _unitOfWork.SaveChangeAsync();

                        _logger.LogInformation($"Created entrance test notification for learner {paymentDTO.LearnerId} for class {paymentDTO.ClassId}");
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, $"Failed to create entrance test notification for learner {paymentDTO.LearnerId}");
                    }

                    _logger.LogInformation($"Learner {paymentDTO.LearnerId} successfully enrolled in class {paymentDTO.ClassId} with payment of {paymentAmount} (10% of total {totalClassPrice})");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"You have successfully enrolled in the class '{classEntity.ClassName}'. Payment of {paymentAmount} (10% of total {totalClassPrice}) has been processed.",
                        Data = new
                        {
                            LearningRegisId = learningRegis.LearningRegisId,
                            LearnerId = paymentDTO.LearnerId,
                            ClassId = paymentDTO.ClassId,
                            AmountPaid = paymentAmount,
                            TotalClassPrice = totalClassPrice,
                            CertificateStatus = classEntity.StartDate <= today ? "Created" : $"Scheduled for {classEntity.StartDate}"
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Detailed error during class enrollment: {Message}", ex.Message);
                    if (ex.InnerException != null)
                    {
                        _logger.LogError("Inner exception: {Message}", ex.InnerException.Message);
                    }

                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing class enrollment with payment");

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to enroll in class: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO> RejectLearningRegisAsync(int learningRegisId, int? responseId)
        {
            try
            {
                _logger.LogInformation($"Starting learning registration rejection process for registration ID: {learningRegisId}");

                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    _logger.LogWarning($"Learning Registration with ID {learningRegisId} not found");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found."
                    };
                }

                if (learningRegis.Status != LearningRegis.Pending)
                {
                    _logger.LogWarning($"Cannot reject registration {learningRegisId} with status {learningRegis.Status}. Only pending registrations can be rejected.");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Cannot reject registration with status {learningRegis.Status}. Only pending registrations can be rejected."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    learningRegis.Status = LearningRegis.Rejected;

                    if (responseId.HasValue)
                    {
                        var response = await _unitOfWork.ResponseRepository.GetByIdAsync(responseId.Value);
                        if (response == null)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Response with ID {responseId.Value} not found."
                            };
                        }

                        learningRegis.ResponseId = responseId.Value;
                    }

                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();


                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation($"Learning registration {learningRegisId} successfully rejected");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Learning Registration rejected successfully.",
                        Data = new
                        {
                            LearningRegisId = learningRegisId,
                            ResponseId = learningRegis.ResponseId
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during rejection of learning registration {learningRegisId}: {ex.Message}");
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing learning registration rejection: {ex.Message}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to reject learning registration: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateLearningPathSessionsAsync(LearningPathSessionsCreateDTO createDTO)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(createDTO.LearningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found."
                    };
                }

                if (createDTO.LearningPathSessions.Count > learningRegis.NumberOfSession)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Number of learning path sessions ({createDTO.LearningPathSessions.Count}) exceeds the number of sessions ({learningRegis.NumberOfSession})."
                    };
                }

                var distinctSessionCount = createDTO.LearningPathSessions
                    .Select(s => s.SessionNumber)
                    .Distinct()
                    .Count();

                if (distinctSessionCount != createDTO.LearningPathSessions.Count)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Duplicate session numbers found in the request."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                var existingSessions = await _unitOfWork.LearningPathSessionRepository
                    .GetByLearningRegisIdAsync(createDTO.LearningRegisId);

                if (existingSessions.Any())
                {
                    foreach (var session in existingSessions)
                    {
                        await _unitOfWork.LearningPathSessionRepository
                            .DeleteAsync(session.LearningPathSessionId);
                    }
                    await _unitOfWork.SaveChangeAsync();
                }

                var learningPathSessions = createDTO.LearningPathSessions.Select(lps => new LearningPathSession
                {
                    LearningRegisId = learningRegis.LearningRegisId,
                    SessionNumber = lps.SessionNumber,
                    Title = lps.Title,
                    Description = lps.Description,
                    IsCompleted = lps.IsCompleted,
                    CreatedAt = DateTime.Now,
                    IsVisible = false
                }).ToList();

                await _unitOfWork.LearningPathSessionRepository.AddRangeAsync(learningPathSessions);
                await _unitOfWork.SaveChangeAsync();

                learningRegis.HasPendingLearningPath = true;
                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                await _unitOfWork.SaveChangeAsync();

                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Learning path sessions created successfully for Learning Registration {createDTO.LearningRegisId}.",
                    Data = createDTO.LearningPathSessions.Count
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to create Learning Path Sessions. " + ex.Message
                };
            }
        }

        private DateOnly GetNextDayOfWeek(DateOnly startDate, DayOfWeeks dayOfWeek)
        {
            int daysToAdd = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;
            if (daysToAdd == 0)
                daysToAdd = 7;

            return startDate.AddDays(daysToAdd);
        }

        private DateOnly GetDateForDayOfWeek(DateOnly startDay, DayOfWeeks targetDay, int weekMultiplier = 0)
        {
            int daysToAdd = ((int)targetDay - (int)startDay.DayOfWeek + 7) % 7;

            daysToAdd += (weekMultiplier * 7);

            return startDay.AddDays(daysToAdd);
        }

        private async Task<object> GetFirstPaymentPeriodInfoAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    return null;
                }

                decimal totalPrice = learningRegis.Price ?? 0;
                decimal firstPaymentAmount = Math.Round(totalPrice * 0.4m, 0);

                var firstPaymentCompleted = false;
                string firstPaymentStatus = "Chưa thanh toán";
                DateTime? firstPaymentDate = null;

                if (learningRegis.Status == LearningRegis.Fourty ||
                    learningRegis.Status == LearningRegis.FourtyFeedbackDone ||
                    learningRegis.Status == LearningRegis.Sixty)
                {
                    _logger.LogInformation($"Learning reg status is {learningRegis.Status}. Setting first payment as completed.");
                    firstPaymentCompleted = true;
                    firstPaymentStatus = "Đã thanh toán";
                }

                if (firstPaymentCompleted)
                {
                    var payments = await _unitOfWork.PaymentsRepository
                        .GetQuery()
                        .Where(p => p.PaymentFor == PaymentFor.LearningRegistration &&
                                   p.Status == PaymentStatus.Completed)
                        .ToListAsync();

                    if (payments != null && payments.Any())
                    {
                        var relevantPayments = new List<(Payment payment, DateTime transactionDate)>();

                        foreach (var payment in payments)
                        {
                            var transaction = await _unitOfWork.WalletTransactionRepository
                                .GetTransactionWithWalletAsync(payment.TransactionId);

                            if (transaction != null &&
                                transaction.Wallet.LearnerId == learningRegis.LearnerId)
                            {
                                if (Math.Abs(payment.AmountPaid - firstPaymentAmount) < 0.1m)
                                {
                                    relevantPayments.Add((payment, transaction.TransactionDate));
                                }
                            }
                        }

                        if (relevantPayments.Any())
                        {
                            var mostRecentPayment = relevantPayments
                                .OrderByDescending(p => p.transactionDate)
                                .FirstOrDefault();

                            firstPaymentDate = mostRecentPayment.transactionDate;
                            _logger.LogInformation($"Found payment date: {firstPaymentDate} for 40% payment of registration {learningRegisId}");
                        }
                    }
                }

                int? firstPaymentRemainingDays = null;
                DateTime? firstPaymentDeadline = null;

                if (learningRegis.Status == LearningRegis.Accepted && learningRegis.PaymentDeadline.HasValue)
                {
                    firstPaymentDeadline = learningRegis.PaymentDeadline;

                    DateTime now = DateTime.Now.Date;
                    DateTime deadline = firstPaymentDeadline.Value.Date;

                    int daysDifference = (deadline - now).Days;

                    if (now.Date == deadline.Date)
                    {
                        firstPaymentRemainingDays = 0;
                    }
                    else
                    {
                        firstPaymentRemainingDays = daysDifference;
                    }

                    if (firstPaymentRemainingDays < 0)
                        firstPaymentRemainingDays = 0;
                }

                return new
                {
                    PaymentPercent = 40,
                    PaymentAmount = firstPaymentAmount,
                    PaymentStatus = firstPaymentStatus,
                    PaymentDeadline = firstPaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentDate = firstPaymentCompleted ? firstPaymentDate?.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    RemainingDays = firstPaymentRemainingDays
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting first payment period info for registration {learningRegisId}");
                return null;
            }
        }

        private async Task<object> GetSecondPaymentPeriodInfoAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    return null;
                }

                decimal totalPrice = learningRegis.Price ?? 0;
                decimal secondPaymentAmount = Math.Round(totalPrice * 0.6m, 0);

                var secondPaymentCompleted = false;
                string secondPaymentStatus = "Chưa thanh toán";
                DateTime? secondPaymentDate = null;

                if (learningRegis.Status == LearningRegis.Sixty)
                {
                    _logger.LogInformation($"Learning reg status is {learningRegis.Status}. Setting second payment as completed.");
                    secondPaymentCompleted = true;
                    secondPaymentStatus = "Đã thanh toán";
                }

                var payments = await _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => p.PaymentFor == PaymentFor.LearningRegistration &&
                              p.Status == PaymentStatus.Completed)
                    .ToListAsync();

                if (payments != null && payments.Any())
                {
                    foreach (var payment in payments)
                    {
                        var transaction = await _unitOfWork.WalletTransactionRepository
                            .GetTransactionWithWalletAsync(payment.TransactionId);

                        if (transaction != null && transaction.Wallet.LearnerId == learningRegis.LearnerId)
                        {
                            if (Math.Abs(payment.AmountPaid - secondPaymentAmount) < 0.1m && secondPaymentDate == null)
                            {
                                secondPaymentDate = transaction.TransactionDate;
                                _logger.LogInformation($"Found payment date: {secondPaymentDate} for 60% payment of registration {learningRegisId}");
                            }
                        }
                    }
                }

                int? secondPaymentRemainingDays = null;
                DateTime? secondPaymentDeadline = null;

                bool isInSecondPaymentPhase =
                    (learningRegis.Status == LearningRegis.FourtyFeedbackDone) &&
                    !secondPaymentCompleted;

                if (isInSecondPaymentPhase && learningRegis.PaymentDeadline.HasValue)
                {
                    secondPaymentDeadline = learningRegis.PaymentDeadline;

                    DateTime now = DateTime.Now.Date;
                    DateTime deadline = secondPaymentDeadline.Value.Date;

                    int daysDifference = (deadline - now).Days;

                    if (now.Date == deadline.Date)
                    {
                        secondPaymentRemainingDays = 0;
                    }
                    else
                    {
                        secondPaymentRemainingDays = daysDifference;
                    }

                    if (secondPaymentRemainingDays < 0)
                        secondPaymentRemainingDays = 0;
                }

                return new
                {
                    PaymentPercent = 60,
                    PaymentAmount = secondPaymentAmount,
                    PaymentStatus = secondPaymentStatus,
                    PaymentDeadline = secondPaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentDate = secondPaymentCompleted ? secondPaymentDate?.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    RemainingDays = secondPaymentRemainingDays
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting second payment period info for registration {learningRegisId}");
                return null;
            }
        }
    }
}
