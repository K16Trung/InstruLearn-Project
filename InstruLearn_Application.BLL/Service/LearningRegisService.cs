using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Certification;
using InstruLearn_Application.Model.Models.DTO.LearnerClass;
using InstruLearn_Application.Model.Models.DTO.LearningPathSession;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

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
                        var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                            .GetByLearningRegisIdAsync(regDto.LearningRegisId);

                        if (learningPathSessions != null && learningPathSessions.Any())
                        {
                            var learningPathSessionDTOs = _mapper.Map<List<LearningPathSessionDTO>>(learningPathSessions);
                            enrichedReg["LearningPath"] = learningPathSessionDTOs;
                        }

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

                var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                    .GetByLearningRegisIdAsync(learningRegisId);

                if (learningPathSessions != null && learningPathSessions.Any())
                {
                    var learningPathSessionDTOs = _mapper.Map<List<LearningPathSessionDTO>>(learningPathSessions);
                    enrichedDict["LearningPath"] = learningPathSessionDTOs;
                }

                enrichedDict["teacherChangeStatus"] = new
                {
                    ChangeTeacherRequested = registration.ChangeTeacherRequested,
                    TeacherChangeProcessed = registration.TeacherChangeProcessed
                };

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
                /*_logger.LogInformation("Starting learning registration process.");

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
                }*/

                _logger.LogInformation("Starting learning registration process.");

                // Check if valid learning days were provided
                if (createLearningRegisDTO.LearningDays == null || !createLearningRegisDTO.LearningDays.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Please select at least one day for learning sessions."
                    };
                }

                // Calculate the potential schedule dates based on requested days of week
                var scheduleDates = new List<DateOnly>();
                var startDate = createLearningRegisDTO.StartDay.Value;
                var daysOfWeek = createLearningRegisDTO.LearningDays.Select(d => (DayOfWeek)d).ToList();

                // For checking purposes, let's look at sessions over the next 8 weeks (enough to catch conflicts)
                for (int week = 0; week < 8; week++)
                {
                    for (int day = 0; day < 7; day++)
                    {
                        var checkDate = startDate.AddDays(week * 7 + day);
                        if (daysOfWeek.Contains(checkDate.DayOfWeek))
                        {
                            scheduleDates.Add(checkDate);
                        }
                    }
                }

                _logger.LogInformation($"Checking potential schedule conflicts across {scheduleDates.Count} session dates");

                foreach (var date in scheduleDates)
                {
                    var scheduleConflict = await _scheduleService.CheckLearnerScheduleConflictAsync(
                        createLearningRegisDTO.LearnerId,
                        date,
                        createLearningRegisDTO.TimeStart,
                        createLearningRegisDTO.TimeLearning);

                    if (!scheduleConflict.IsSucceed)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Schedule conflict detected on {date.ToString("yyyy-MM-dd")}: {scheduleConflict.Message}"
                        };
                    }
                }

                var existingRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(r =>
                        r.LearnerId == createLearningRegisDTO.LearnerId &&
                        r.TimeStart == createLearningRegisDTO.TimeStart &&
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

                    /*if (!string.IsNullOrEmpty(createLearningRegisDTO.SelfAssessment))
                    {
                        learningRegis.SelfAssessment = createLearningRegisDTO.SelfAssessment;
                    }*/

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

                    // Find the original registration from the collection to access the teacher change flags
                    var originalRegistration = registrations.FirstOrDefault(r => r.LearningRegisId == regDto.LearningRegisId);
                    if (originalRegistration != null)
                    {
                        enrichedReg["teacherChangeStatus"] = new
                        {
                            ChangeTeacherRequested = originalRegistration.ChangeTeacherRequested,
                            TeacherChangeProcessed = originalRegistration.TeacherChangeProcessed
                        };
                    }

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

                // Check if learner is already enrolled in this class
                var existingEnrollments = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .AnyAsync(lr =>
                        lr.LearnerId == paymentDTO.LearnerId &&
                        lr.ClassId == paymentDTO.ClassId &&
                        (lr.Status == LearningRegis.Pending || lr.Status == LearningRegis.Accepted ||
                         lr.Status == LearningRegis.Fourty || lr.Status == LearningRegis.Sixty));

                if (existingEnrollments)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "You already have an enrollment or pending enrollment for this class."
                    };
                }

                // Get class information
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

                if (classEntity.Status != ClassStatus.Scheduled)
                {
                    string statusMessage = classEntity.Status switch
                    {
                        ClassStatus.OnTestDay => "class is on test day",
                        ClassStatus.Ongoing => "class has already started",
                        ClassStatus.Completed => "class has already completed",
                        _ => "class is not in an enrollable state"
                    };

                    _logger.LogWarning($"Learner {paymentDTO.LearnerId} attempted to join class {paymentDTO.ClassId} but {statusMessage}");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Cannot enroll in this class as the {statusMessage}. Only classes with 'Scheduled' status are available for enrollment."
                    };
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                if (classEntity.TestDay == today)
                {
                    _logger.LogWarning($"Learner {paymentDTO.LearnerId} attempted to join class {paymentDTO.ClassId} on test day");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không thể đăng ký vào lớp học này vào ngày hôm nay vì hôm nay là ngày kiểm tra đầu vào. Vui lòng liên hệ trung tâm để biết thông tin về các kỳ thi tiếp theo."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    int? levelId = classEntity.LevelId;
                    if (!levelId.HasValue)
                    {
                        _logger.LogWarning($"Class with ID {paymentDTO.ClassId} doesn't have an associated level");
                    }
                    else
                    {
                        _logger.LogInformation($"Using level ID {levelId} from class {paymentDTO.ClassId}");
                    }

                    // Get learner information
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

                    // Check for existing enrollment in Learner_Classes table
                    var existingLearnerClassEnrollment = await _unitOfWork.dbContext.Learner_Classes
                        .FirstOrDefaultAsync(lc => lc.LearnerId == paymentDTO.LearnerId && lc.ClassId == paymentDTO.ClassId);

                    if (existingLearnerClassEnrollment != null)
                    {
                        _logger.LogWarning($"Learner {paymentDTO.LearnerId} is already enrolled in class {paymentDTO.ClassId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "You are already enrolled in this class."
                        };
                    }

                    // Calculate payment amount
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
                    decimal paymentAmount = Math.Round(totalClassPrice * 0.1m, 2);

                    _logger.LogInformation($"Class price calculation: {pricePerDay} per day × {classEntity.totalDays} days = {totalClassPrice} total. 10% payment: {paymentAmount}");

                    // Get registration type for center classes
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

                    // Check wallet balance
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

                    // Process payment
                    wallet.Balance -= paymentAmount;
                    await _unitOfWork.WalletRepository.UpdateAsync(wallet);
                    await _unitOfWork.SaveChangeAsync();

                    // Create wallet transaction record
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

                    // Create learning registration record
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
                        AcceptedDate = DateTime.UtcNow,
                        Price = totalClassPrice,
                        NumberOfSession = classEntity.totalDays,
                        TimeStart = classEntity.ClassTime,
                        TimeLearning = 120,
                        StartDay = classEntity.StartDate,
                        VideoUrl = string.Empty,
                        LearningRequest = string.Empty,
                    };

                    await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    // Create learner class enrollment
                    var learnerClass = new Learner_class
                    {
                        LearnerId = paymentDTO.LearnerId,
                        ClassId = paymentDTO.ClassId
                    };

                    _unitOfWork.dbContext.Learner_Classes.Add(learnerClass);
                    await _unitOfWork.SaveChangeAsync();

                    // Handle certificate creation based on class start date
                    if (classEntity.StartDate == today)
                    {
                        _logger.LogInformation($"Class starts today. Creating temporary certificate for learner {paymentDTO.LearnerId} in class {paymentDTO.ClassId}");

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
                                CertificationName = $"[TEMPORARY] Center Learning Certificate - {classEntity.ClassName}",
                                TeacherName = teacherName,
                                Subject = majorName
                            };

                            var certificationService = _serviceProvider.GetRequiredService<ICertificationService>();
                            var certResult = await certificationService.CreateCertificationAsync(createCertificationDTO);

                            if (certResult.IsSucceed)
                            {
                                _logger.LogInformation($"Temporary certificate created successfully for learner {paymentDTO.LearnerId} in class {paymentDTO.ClassId}");

                                // Schedule a notification for certificate verification after attendance is recorded
                                var staffNotification = new StaffNotification
                                {
                                    Title = "Certificate Eligibility Verification Required",
                                    Message = $"Learner {learner.FullName} (ID: {paymentDTO.LearnerId}) received a temporary certificate for class {classEntity.ClassName} (ID: {paymentDTO.ClassId}). Verify 75% attendance before finalizing certificate.",
                                    LearnerId = paymentDTO.LearnerId,
                                    CreatedAt = DateTime.Now.AddDays(classEntity.totalDays / 2), // Set future date to verify halfway through course
                                    Status = NotificationStatus.Unread,
                                    Type = NotificationType.Certificate
                                };

                                await _unitOfWork.StaffNotificationRepository.AddAsync(staffNotification);
                                await _unitOfWork.SaveChangeAsync();
                            }
                            else
                            {
                                _logger.LogWarning($"Failed to create temporary certificate: {certResult.Message}");
                            }
                        }
                        catch (Exception certEx)
                        {
                            _logger.LogError(certEx, $"Error creating temporary certificate for learner {paymentDTO.LearnerId} in class {paymentDTO.ClassId}");
                        }
                    }
                    else if (classEntity.StartDate < today)
                    {
                        _logger.LogInformation($"Class has already started on {classEntity.StartDate}, but learner is joining today ({today}). No certificate will be created immediately.");

                        // Create notification to check attendance later
                        var staffNotification = new StaffNotification
                        {
                            Title = "Late Enrollment - Certificate Eligibility Check Needed",
                            Message = $"Learner {learner.FullName} (ID: {paymentDTO.LearnerId}) joined class {classEntity.ClassName} (ID: {paymentDTO.ClassId}) after the start date. The class started on {classEntity.StartDate} and learner joined on {today}. Check attendance before issuing certificate.",
                            LearnerId = paymentDTO.LearnerId,
                            CreatedAt = DateTime.Now.AddDays(classEntity.totalDays / 2),
                            Status = NotificationStatus.Unread,
                            Type = NotificationType.Certificate
                        };

                        await _unitOfWork.StaffNotificationRepository.AddAsync(staffNotification);
                        await _unitOfWork.SaveChangeAsync();
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
                    }

                    // Create schedules for the learner
                    await CreateLearnerSchedulesForClass(paymentDTO.LearnerId, paymentDTO.ClassId, classEntity, learningRegis);

                    // Create entrance test notification
                    try
                    {
                        string formattedClassTime = classEntity.ClassTime.ToString("HH:mm");
                        string formattedTestDay = classEntity.TestDay.ToString("dd/MM/yyyy");

                        string notificationMessage =
                            "<p>Chào bạn,</p>" +
                            "<p>Cảm ơn bạn đã đăng ký tham gia lớp học " + classEntity.ClassName + " tại InstruLearn.</p>" +
                            "<p>Để hoàn tất việc xếp lớp, bạn vui lòng đến trung tâm InstruLearn vào lúc " + formattedClassTime + " ngày " + formattedTestDay + " để thực hiện kiểm tra chất lượng đầu vào.</p>" +
                            "<p>Việc kiểm tra này giúp chúng tôi sắp xếp lớp phù hợp với trình độ hiện tại của bạn và thực hiện thanh toán.</p>" +
                            "<p><strong>Lưu ý:</strong><br>" +
                            "Học viên vui lòng bỏ qua thông báo này nếu:</p>" +
                            "<ul>" +
                            "<li>Học viên đã thực hiện kiểm tra chất lượng đầu vào.</li>" +
                            "<li>Đã được chuyển lớp do chưa phù hợp với trình độ lớp đã đăng ký.</li>" +
                            "<li>Đã được chuyển lớp để phù hợp hơn với năng lực hiện tại.</li>" +
                            "</ul>" +
                            "<p><strong>Địa chỉ:</strong> 935 Huỳnh Tấn Phát, Phú Thuận, Quận 7, TP.HCM</p>" +
                            "<p>Trân trọng,<br>InstruLearn</p>";

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

                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation($"Learner {paymentDTO.LearnerId} successfully enrolled in class {paymentDTO.ClassId} with payment of {paymentAmount} (10% of total {totalClassPrice})");

                    // Updated certificate status message based on when learner joins relative to class start date
                    string certificateStatus;
                    if (classEntity.StartDate == today)
                    {
                        certificateStatus = "Temporary Certificate Created";
                    }
                    else if (classEntity.StartDate < today)
                    {
                        certificateStatus = "Will be evaluated based on attendance";
                    }
                    else
                    {
                        certificateStatus = $"Scheduled for {classEntity.StartDate}";
                    }

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
                            CertificateStatus = certificateStatus
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

        // Helper method to create schedules for a learner in a class
        private async Task CreateLearnerSchedulesForClass(int learnerId, int classId, Class classEntity, Learning_Registration learningRegis)
        {
            // Check for existing schedules with learner IDs in this class
            var existingLearnerSchedules = await _unitOfWork.ScheduleRepository.GetQuery()
                .Where(s => s.ClassId == classId &&
                           s.TeacherId == classEntity.TeacherId &&
                           s.LearnerId != null)
                .OrderBy(s => s.StartDay)
                .ToListAsync();

            if (existingLearnerSchedules != null && existingLearnerSchedules.Any())
            {
                _logger.LogInformation($"Found {existingLearnerSchedules.Count} existing schedules for other learners in class {classId}");

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
                        LearnerId = learnerId,
                        ClassId = classId,
                        LearningRegisId = learningRegis.LearningRegisId,
                        TeacherId = classEntity.TeacherId,
                        StartDay = existingSchedule.StartDay,
                        TimeStart = classEntity.ClassTime,
                        TimeEnd = classEntity.ClassTime.AddHours(2),
                        Mode = ScheduleMode.Center,
                        AttendanceStatus = AttendanceStatus.NotYet
                    };

                    newSchedules.Add(newSchedule);
                }

                await _unitOfWork.ScheduleRepository.AddRangeAsync(newSchedules);
                await _unitOfWork.SaveChangeAsync();
                return;
            }

            // Check for teacher schedules without learner IDs
            var existingTeacherSchedules = await _unitOfWork.ScheduleRepository.GetQuery()
                .Where(s => s.ClassId == classId &&
                           s.TeacherId == classEntity.TeacherId &&
                           s.LearnerId == null)
                .OrderBy(s => s.StartDay)
                .ToListAsync();

            if (existingTeacherSchedules != null && existingTeacherSchedules.Any())
            {
                _logger.LogInformation($"Found {existingTeacherSchedules.Count} existing teacher schedules for class {classId}");

                int schedulesUsed = 0;
                var learnerSchedules = new List<Schedules>();

                foreach (var teacherSchedule in existingTeacherSchedules.OrderBy(s => s.StartDay))
                {
                    if (schedulesUsed >= classEntity.totalDays)
                        break;

                    var learnerSchedule = new Schedules
                    {
                        LearnerId = learnerId,
                        ClassId = classId,
                        LearningRegisId = learningRegis.LearningRegisId,
                        TeacherId = classEntity.TeacherId,
                        StartDay = teacherSchedule.StartDay,
                        TimeStart = classEntity.ClassTime,
                        TimeEnd = classEntity.ClassTime.AddHours(2),
                        Mode = ScheduleMode.Center,
                        AttendanceStatus = AttendanceStatus.NotYet
                    };

                    learnerSchedules.Add(learnerSchedule);
                    schedulesUsed++;
                }

                await _unitOfWork.ScheduleRepository.AddRangeAsync(learnerSchedules);
                await _unitOfWork.SaveChangeAsync();
                return;
            }

            // No existing schedules found, create new ones based on class days
            _logger.LogWarning($"No existing schedules found for class {classId}, creating new ones");

            var classDays = await _unitOfWork.ClassDayRepository.GetQuery()
                .Where(cd => cd.ClassId == classId)
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
                        LearnerId = learnerId,
                        ClassId = classId,
                        LearningRegisId = learningRegis.LearningRegisId,
                        TeacherId = classEntity.TeacherId,
                        StartDay = scheduleDay,
                        TimeStart = classEntity.ClassTime,
                        TimeEnd = classEntity.ClassTime.AddHours(2),
                        Mode = ScheduleMode.Center,
                        AttendanceStatus = AttendanceStatus.NotYet
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
                _logger.LogWarning($"No class days found for class {classId}. Enrollment may be incomplete.");
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
                    string responseDescription = "No specific reason provided.";
                    string responseTypeName = "Other";

                    if (responseId.HasValue)
                    {
                        // Get response with its response type to include in notification
                        var response = await _unitOfWork.ResponseRepository.GetWithIncludesAsync(
                            r => r.ResponseId == responseId.Value,
                            "ResponseType");

                        if (response == null || !response.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Response with ID {responseId.Value} not found."
                            };
                        }

                        var selectedResponse = response.First();
                        learningRegis.ResponseId = responseId.Value;
                        responseDescription = selectedResponse.ResponseName ?? responseDescription;

                        // Get the response type name if available
                        if (selectedResponse.ResponseType != null)
                        {
                            responseTypeName = selectedResponse.ResponseType.ResponseTypeName;
                        }
                    }

                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                    // Get learner details for notification
                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learningRegis.LearnerId);
                    if (learner != null)
                    {
                        // Create notification for the learner
                        var notification = new StaffNotification
                        {
                            LearnerId = learningRegis.LearnerId,
                            LearningRegisId = learningRegis.LearningRegisId,
                            Type = NotificationType.RegistrationRejected,
                            Status = NotificationStatus.Unread,
                            CreatedAt = DateTime.Now,
                            Title = "Đơn đăng kí bị từ chối",
                            Message = $"Đơn đăng kí của bạn (ID: {learningRegis.LearningRegisId}) đã bị từ chối. Lý do: {responseDescription}"
                        };

                        await _unitOfWork.StaffNotificationRepository.AddAsync(notification);

                        // Send email notification if learner has an email address
                        var account = learner.Account != null ?
                            learner.Account :
                            await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);

                        if (account != null && !string.IsNullOrEmpty(account.Email))
                        {
                            try
                            {
                                string subject = "Đơn đăng kí bị từ chối";
                                string body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                                <h2 style='color: #333;'>Xin chào {learner.FullName},</h2>
                                
                                <p>Chúng tôi rất tiếc phải thông báo rằng đơn đăng ký học tập của bạn đã bị từ chối.</p>
                                
                                <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                                    <h3 style='margin-top: 0; color: #333;'>Chi tiết đơn đăng ký:</h3>
                                    <p><strong>ID đăng ký học:</strong> {learningRegis.LearningRegisId}</p>
                                    <p><strong>Lý do từ chối:</strong> {responseDescription}</p>
                                </div>
                                
                                <p>Nếu bạn có bất kỳ câu hỏi nào hoặc muốn biết thêm thông tin, vui lòng liên hệ với nhóm hỗ trợ của chúng tôi.</p>
                                
                                <p>Bạn có thể tạo đơn đăng ký mới hoặc cập nhật thông tin đơn đăng ký hiện tại để nộp lại.</p>
                                
                                <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                            </div>
                        </body>
                        </html>";

                                // Send email notification
                                await _emailService.SendEmailAsync(account.Email, subject, body, true);
                                _logger.LogInformation($"Rejection email sent to {account.Email} for learning registration {learningRegisId}");
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, $"Error sending rejection email for learning registration {learningRegisId}");
                                // Continue with transaction even if email fails
                            }
                        }
                    }
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
                    learningRegis.Status == LearningRegis.Sixty ||
                    learningRegis.Status == LearningRegis.Payment60Rejected)
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

                bool isOverdue = firstPaymentDeadline.HasValue && !firstPaymentCompleted && DateTime.Now > firstPaymentDeadline.Value;

                return new
                {
                    PaymentPercent = 40,
                    PaymentAmount = firstPaymentAmount,
                    PaymentStatus = firstPaymentStatus,
                    PaymentDeadline = firstPaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentDate = firstPaymentCompleted ? firstPaymentDate?.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    RemainingDays = firstPaymentRemainingDays,
                    IsOverdue = isOverdue
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

                if (isInSecondPaymentPhase)
                {
                    // If waiting for teacher change, provide appropriate status
                    if (learningRegis.ChangeTeacherRequested && !learningRegis.TeacherChangeProcessed)
                    {
                        secondPaymentStatus = "Đang chờ thay đổi giáo viên";
                        _logger.LogInformation($"Learning registration {learningRegisId} is waiting for teacher change");
                    }
                    else if (learningRegis.PaymentDeadline.HasValue)
                    {
                        // Normal case - deadline is set
                        secondPaymentDeadline = learningRegis.PaymentDeadline;

                        DateTime now = DateTime.Now;
                        DateTime deadline = secondPaymentDeadline.Value;

                        int daysDifference = (deadline.Date - now.Date).Days;

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

                        if (daysDifference < 0 && !secondPaymentCompleted)
                        {
                            secondPaymentStatus = "Đã quá hạn thanh toán 60%";
                        }
                    }
                }

                bool isOverdue = secondPaymentDeadline.HasValue && !secondPaymentCompleted && DateTime.Now > secondPaymentDeadline.Value;

                return new
                {
                    PaymentPercent = 60,
                    PaymentAmount = secondPaymentAmount,
                    PaymentStatus = secondPaymentStatus,
                    PaymentDeadline = secondPaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentDate = secondPaymentCompleted ? secondPaymentDate?.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    RemainingDays = secondPaymentRemainingDays,
                    IsOverdue = isOverdue,
                    AwaitingTeacherChange = isInSecondPaymentPhase && learningRegis.ChangeTeacherRequested && !learningRegis.TeacherChangeProcessed
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
