﻿using AutoMapper;
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

                        // Add learner address and account details
                        if (registration.LearnerId > 0)
                        {
                            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(registration.LearnerId);
                            if (learner != null && !string.IsNullOrEmpty(learner.AccountId))
                            {
                                var account = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
                                if (account != null)
                                {
                                    enrichedReg["learnerAddress"] = account.Address;
                                }
                            }
                        }

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
                            _logger.LogInformation($"Đang xử lý lịch trình cho đăng ký ID: {registration.LearningRegisId}. Đã tìm thấy {registration.Schedules.Count} lịch trình tổng cộng");

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
                            _logger.LogInformation($"Không tìm thấy lịch trình cho đăng ký ID: {registration.LearningRegisId}. Đang tạo ngày dựa trên các ngày học.");

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
                            _logger.LogWarning($"Không thể tính toán ngày buổi học cho đăng ký ID: {registration.LearningRegisId}");
                            regDto.SessionDates = new List<string>();
                        }

                        enrichedReg["SessionDates"] = regDto.SessionDates;
                    }

                    enrichedRegistrations.Add(enrichedReg);
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy tất cả đăng ký học tập thành công.",
                    Data = enrichedRegistrations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy đăng ký học tập cùng thông tin thanh toán");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy danh sách đăng ký học tập: {ex.Message}"
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
                        Message = "Không tìm thấy đăng ký học tập.",
                        Data = null
                    };
                }

                var dto = _mapper.Map<OneOnOneRegisDTO>(registration);

                var registrationDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    JsonSerializer.Serialize(dto));

                var firstPaymentPeriod = await GetFirstPaymentPeriodInfoAsync(learningRegisId);
                var secondPaymentPeriod = await GetSecondPaymentPeriodInfoAsync(learningRegisId);

                var enrichedReg = new Dictionary<string, object>();
                foreach (var kvp in registrationDict)
                {
                    enrichedReg[kvp.Key] = JsonSerializer.Deserialize<object>(kvp.Value.GetRawText());
                }

                enrichedReg["firstPaymentPeriod"] = firstPaymentPeriod;
                enrichedReg["secondPaymentPeriod"] = secondPaymentPeriod;

                if (registration.LearnerId > 0)
                {
                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(registration.LearnerId);
                    if (learner != null && !string.IsNullOrEmpty(learner.AccountId))
                    {
                        var account = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
                        if (account != null)
                        {
                            enrichedReg["learnerAddress"] = account.Address;
                            enrichedReg["accountDetails"] = new
                            {
                                Address = account.Address,
                                PhoneNumber = account.PhoneNumber,
                                Gender = account.Gender,
                                Email = account.Email,
                                Avatar = account.Avatar
                            };
                        }
                    }
                }

                var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                    .GetByLearningRegisIdAsync(learningRegisId);

                if (learningPathSessions != null && learningPathSessions.Any())
                {
                    var learningPathSessionDTOs = _mapper.Map<List<LearningPathSessionDTO>>(learningPathSessions);
                    enrichedReg["LearningPath"] = learningPathSessionDTOs;
                }

                enrichedReg["teacherChangeStatus"] = new
                {
                    ChangeTeacherRequested = registration.ChangeTeacherRequested,
                    TeacherChangeProcessed = registration.TeacherChangeProcessed
                };

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Lấy thông tin đăng ký học tập thành công.",
                    Data = enrichedReg
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy đăng ký học tập {learningRegisId} cùng thông tin thanh toán");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy thông tin đăng ký học tập: {ex.Message}",
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
                    Message = $"Đã lọc thành công các đăng ký học tập cho giáo viên ID {teacherId}.",
                    Data = registrationDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy đăng ký cho giáo viên {teacherId}: {ex.Message}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy danh sách đăng ký học tập: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateLearningRegisAsync(CreateLearningRegisDTO createLearningRegisDTO)
        {
            try
            {
                _logger.LogInformation("Bắt đầu quá trình đăng ký học tập.");

                if (createLearningRegisDTO.LearningDays == null || !createLearningRegisDTO.LearningDays.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Vui lòng chọn ít nhất một ngày cho buổi học."
                    };
                }

                var scheduleDates = new List<DateOnly>();
                var startDate = createLearningRegisDTO.StartDay.Value;
                var daysOfWeek = createLearningRegisDTO.LearningDays.Select(d => (DayOfWeek)d).ToList();

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

                _logger.LogInformation($"Đang kiểm tra xung đột lịch trình tiềm năng qua {scheduleDates.Count} ngày buổi học");

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
                            Message = $"Phát hiện xung đột lịch học vào ngày {date.ToString("yyyy-MM-dd")}: {scheduleConflict.Message}"
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
                        Message = "Bạn đã có một đăng ký đang chờ xử lý cho môn học này. Vui lòng đợi cho đến khi nó được xử lý trước khi tạo đăng ký mới."
                    };
                }

                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    const string depositConfigKey = "RegistrationDepositAmount";
                    var depositConfig = await _unitOfWork.SystemConfigurationRepository.GetByKeyAsync(depositConfigKey);

                    decimal depositAmount = 50000;

                    if (depositConfig != null && decimal.TryParse(depositConfig.Value, out decimal configValue))
                    {
                        depositAmount = configValue;
                        _logger.LogInformation($"Sử dụng số tiền đặt cọc đã cấu hình: {depositAmount}");
                    }
                    else
                    {
                        _logger.LogInformation($"Sử dụng số tiền đặt cọc mặc định: {depositAmount}");
                    }

                    var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == createLearningRegisDTO.LearnerId);

                    if (wallet == null)
                    {
                        _logger.LogWarning($"Không tìm thấy ví cho học viên: {createLearningRegisDTO.LearnerId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Không tìm thấy ví tiền cho học viên."
                        };
                    }

                    _logger.LogInformation($"Đã tìm thấy ví cho học viên: {createLearningRegisDTO.LearnerId}, số dư: {wallet.Balance}");

                    if (wallet.Balance < depositAmount)
                    {
                        _logger.LogWarning($"Số dư không đủ cho học viên: {createLearningRegisDTO.LearnerId}. Số dư hiện tại: {wallet.Balance}, Số tiền đặt cọc yêu cầu: {depositAmount}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Số dư trong ví không đủ. Cần {depositAmount} VND để đặt cọc đăng ký."
                        };
                    }

                    wallet.Balance -= depositAmount;
                    await _unitOfWork.WalletRepository.UpdateAsync(wallet);

                    if (createLearningRegisDTO.TimeLearning != 45 && createLearningRegisDTO.TimeLearning != 60 && createLearningRegisDTO.TimeLearning != 90 && createLearningRegisDTO.TimeLearning != 120)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Thời lượng học không hợp lệ. Vui lòng chọn 45, 60, 90 hoặc 120 phút."
                        };
                    }

                    var learningRegis = _mapper.Map<Learning_Registration>(createLearningRegisDTO);
                    learningRegis.Status = LearningRegis.Pending;

                    var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    learningRegis.RequestDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

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
                        Amount = depositAmount,
                        TransactionType = TransactionType.Payment,
                        Status = Model.Enum.TransactionStatus.Complete,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);
                    await _unitOfWork.SaveChangeAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation($"Đăng ký học tập đã được thêm thành công. Số dư ví đã được cập nhật với số tiền đặt cọc: {depositAmount}");

                    var timeEnd = learningRegis.TimeStart.AddMinutes(createLearningRegisDTO.TimeLearning);
                    var timeEndFormatted = timeEnd.ToString("HH:mm");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Đăng ký học tập đã được thêm thành công. Đã khấu trừ {depositAmount} VND tiền đặt cọc từ ví của bạn. Trạng thái được đặt là Đang chờ xử lý.",
                        Data = new
                        {
                            LearningRegisId = learningRegis.LearningRegisId,
                            DepositAmount = depositAmount
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình xử lý đăng ký học tập.");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Đã xảy ra lỗi: {ex.Message}"
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
                    Message = "Không tìm thấy đăng ký học tập."
                };
            }
            await _unitOfWork.LearningRegisRepository.DeleteAsync(learningRegisId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Xóa đăng ký học tập thành công."
            };
        }

        public async Task<ResponseDTO> GetAllPendingRegistrationsAsync()
        {
            var pendingRegistrations = await _learningRegisRepository.GetPendingRegistrationsAsync();
            var pendingDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(pendingRegistrations);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Lấy danh sách đăng ký học tập đang chờ xử lý thành công.",
                Data = pendingDtos
            };
        }

        public async Task<ResponseDTO> GetRegistrationsByLearnerIdAsync(int learnerId)
        {
            try
            {
                var registrations = await _learningRegisRepository.GetRegistrationsByLearnerIdAsync(learnerId);
                var registrationDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(registrations).ToList();

                string learnerAddress = null;
                Dictionary<string, object> accountDetails = null;
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
                if (learner != null && !string.IsNullOrEmpty(learner.AccountId))
                {
                    var account = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
                    if (account != null)
                    {
                        learnerAddress = account.Address;
                        accountDetails = new Dictionary<string, object>
                        {
                            ["Address"] = account.Address,
                            ["PhoneNumber"] = account.PhoneNumber,
                            ["Gender"] = account.Gender,
                            ["Email"] = account.Email,
                            ["Avatar"] = account.Avatar
                        };
                    }
                }

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

                    if (learnerAddress != null)
                    {
                        enrichedReg["learnerAddress"] = learnerAddress;
                    }

                    if (accountDetails != null)
                    {
                        enrichedReg["accountDetails"] = accountDetails;
                    }

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
                    Message = $"Đã lấy tất cả đăng ký học tập cho học viên ID {learnerId} thành công.",
                    Data = enrichedRegistrations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy đăng ký học tập cho học viên {learnerId} cùng thông tin thanh toán");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy danh sách đăng ký học tập: {ex.Message}"
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
                        Message = "Không tìm thấy đăng ký học tập."
                    };
                }

                var levelAssigned = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(updateDTO.LevelId);
                if (levelAssigned == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy cấp độ được chỉ định."
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
                            <a href='http://localhost:3000/profile/registration-detail/{learningRegis.LearningRegisId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
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
                    Message = $"Cập nhật đăng ký học tập thành công với tổng giá {totalPrice:F2} VND. Đã gửi email thông báo đến học viên.",
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
                    Message = "Không thể cập nhật đăng ký học tập. " + ex.Message
                };
            }
        }

        public async Task<ResponseDTO> JoinClassWithWalletPaymentAsync(LearnerClassPaymentDTO paymentDTO)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu quá trình đăng ký lớp học cho học viên ID: {paymentDTO.LearnerId}, lớp ID: {paymentDTO.ClassId}");
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
                        (lr.Status == LearningRegis.Pending || lr.Status == LearningRegis.Accepted ||
                         lr.Status == LearningRegis.Fourty || lr.Status == LearningRegis.Sixty));

                if (existingEnrollments)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Bạn đã có đăng ký hoặc đăng ký đang chờ xử lý cho lớp học này."
                    };
                }

                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(paymentDTO.ClassId);
                if (classEntity == null)
                {
                    _logger.LogWarning($"Không tìm thấy lớp với ID {paymentDTO.ClassId}");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Không tìm thấy lớp học với ID {paymentDTO.ClassId}."
                    };
                }

                if (classEntity.Status != ClassStatus.Scheduled)
                {
                    string statusMessage = classEntity.Status switch
                    {
                        ClassStatus.OnTestDay => "lớp học đang trong ngày kiểm tra",
                        ClassStatus.Ongoing => "lớp học đã bắt đầu",
                        ClassStatus.Completed => "lớp học đã kết thúc",
                        _ => "lớp học không ở trạng thái có thể đăng ký"
                    };

                    _logger.LogWarning($"Học viên {paymentDTO.LearnerId} đã cố gắng tham gia lớp {paymentDTO.ClassId} nhưng {statusMessage}");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Không thể đăng ký vào lớp học này vì {statusMessage}. Chỉ những lớp học có trạng thái 'Đã lên lịch' mới có thể đăng ký."
                    };
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                if (classEntity.TestDay == today)
                {
                    _logger.LogWarning($"Học viên {paymentDTO.LearnerId} đã cố gắng tham gia lớp {paymentDTO.ClassId} vào ngày kiểm tra");
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
                        _logger.LogWarning($"Lớp học với ID {paymentDTO.ClassId} không có cấp độ liên kết");
                    }
                    else
                    {
                        _logger.LogInformation($"Sử dụng cấp độ ID {levelId} từ lớp {paymentDTO.ClassId}");
                    }

                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(paymentDTO.LearnerId);
                    if (learner == null)
                    {
                        _logger.LogWarning($"Không tìm thấy học viên với ID {paymentDTO.LearnerId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Không tìm thấy học viên với ID {paymentDTO.LearnerId}."
                        };
                    }

                    var existingLearnerClassEnrollment = await _unitOfWork.dbContext.Learner_Classes
                        .FirstOrDefaultAsync(lc => lc.LearnerId == paymentDTO.LearnerId && lc.ClassId == paymentDTO.ClassId);

                    if (existingLearnerClassEnrollment != null)
                    {
                        _logger.LogWarning($"Học viên {paymentDTO.LearnerId} đã đăng ký vào lớp {paymentDTO.ClassId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Bạn đã đăng ký vào lớp này."
                        };
                    }

                    decimal pricePerDay = classEntity.Price;
                    if (pricePerDay <= 0)
                    {
                        _logger.LogWarning($"Giá không hợp lệ cho lớp {paymentDTO.ClassId}: {pricePerDay}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Giá lớp học không hợp lệ."
                        };
                    }

                    decimal totalClassPrice = pricePerDay * classEntity.totalDays;
                    decimal paymentAmount = Math.Round(totalClassPrice * 0.1m, 2);

                    _logger.LogInformation($"Tính giá lớp học: {pricePerDay} mỗi ngày × {classEntity.totalDays} ngày = {totalClassPrice} tổng cộng. Thanh toán 10%: {paymentAmount}");

                    var classRegisType = await _unitOfWork.LearningRegisTypeRepository.GetQuery()
                        .FirstOrDefaultAsync(rt => rt.RegisTypeName.Contains("Center"));

                    if (classRegisType == null)
                    {
                        _logger.LogWarning("Không tìm thấy loại đăng ký lớp học trong database");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Không tìm thấy loại đăng ký lớp học trong hệ thống."
                        };
                    }

                    var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == paymentDTO.LearnerId);
                    if (wallet == null)
                    {
                        _logger.LogWarning($"Không tìm thấy ví cho học viên {paymentDTO.LearnerId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Không tìm thấy ví cho tài khoản của bạn."
                        };
                    }

                    if (wallet.Balance < paymentAmount)
                    {
                        _logger.LogWarning($"Số dư không đủ cho học viên {paymentDTO.LearnerId}. Bắt buộc: {paymentAmount}, Có sẵn: {wallet.Balance}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Số dư không đủ. Bắt buộc: {paymentAmount} (10% tổng số {totalClassPrice}), Có sẵn: {wallet.Balance}"
                        };
                    }

                    wallet.Balance -= paymentAmount;
                    await _unitOfWork.WalletRepository.UpdateAsync(wallet);
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

                    var learnerClass = new Learner_class
                    {
                        LearnerId = paymentDTO.LearnerId,
                        ClassId = paymentDTO.ClassId
                    };

                    _unitOfWork.dbContext.Learner_Classes.Add(learnerClass);
                    await _unitOfWork.SaveChangeAsync();

                    if (classEntity.StartDate == today)
                    {
                        _logger.LogInformation($"Lớp học bắt đầu hôm nay. Tạo chứng chỉ tạm thời cho học viên trong lớp");

                        try
                        {
                            string teacherName = "Giáo viên không xác định";
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

                            string majorName = "Môn học không xác định";
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
                                CertificationName = $"[TẠM THỜI] Chứng chỉ học tập tại trung tâm - {classEntity.ClassName}",
                                TeacherName = teacherName,
                                Subject = majorName
                            };

                            var certificationService = _serviceProvider.GetRequiredService<ICertificationService>();
                            var certResult = await certificationService.CreateCertificationAsync(createCertificationDTO);

                            if (certResult.IsSucceed)
                            {
                                _logger.LogInformation($"Đã tạo thành công chứng chỉ tạm thời cho học viên {paymentDTO.LearnerId} trong lớp {paymentDTO.ClassId}");

                                var staffNotification = new StaffNotification
                                {
                                    Title = "Yêu cầu xác minh đủ điều kiện cấp chứng chỉ",
                                    Message = $"Học viên {learner.FullName} đã nhận được chứng chỉ tạm thời cho lớp học {classEntity.ClassName}. Xác minh 75% sự tham dự trước khi hoàn thiện chứng chỉ.",
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
                                _logger.LogWarning($"Không thể tạo chứng chỉ tạm thời: {certResult.Message}");
                            }
                        }
                        catch (Exception certEx)
                        {
                            _logger.LogError(certEx, $"Lỗi khi tạo chứng chỉ tạm thời cho học viên {paymentDTO.LearnerId} trong lớp {paymentDTO.ClassId}");
                        }
                    }
                    else if (classEntity.StartDate < today)
                    {
                        _logger.LogInformation($"Lớp học đã bắt đầu vào {classEntity.StartDate}, nhưng học viên sẽ tham gia vào hôm nay ({today}). Sẽ không có chứng chỉ nào được tạo ngay lập tức.");

                        var staffNotification = new StaffNotification
                        {
                            Title = "Đăng ký trễ - Cần kiểm tra điều kiện cấp chứng chỉ",
                            Message = $"Học viên {learner.FullName} đã tham gia lớp {classEntity.ClassName} sau ngày bắt đầu. Lớp học bắt đầu vào {classEntity.StartDate} và học viên đã tham gia vào {today}. Kiểm tra điểm danh trước khi cấp chứng chỉ.",
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
                        _logger.LogInformation($"Lớp học bắt đầu vào {classEntity.StartDate}. Tạo thông báo cho việc tạo chứng chỉ trong tương lai");

                        var staffNotification = new StaffNotification
                        {
                            Title = "Đã lên lịch tạo chứng chỉ",
                            Message = $"Tạo chứng chỉ cho học viên {learner.FullName} trong lớp {classEntity.ClassName} vào ngày bắt đầu {classEntity.StartDate}",
                            LearnerId = paymentDTO.LearnerId,
                            CreatedAt = DateTime.Now,
                            Status = NotificationStatus.Unread,
                            Type = NotificationType.Certificate
                        };

                        await _unitOfWork.StaffNotificationRepository.AddAsync(staffNotification);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    await CreateLearnerSchedulesForClass(paymentDTO.LearnerId, paymentDTO.ClassId, classEntity, learningRegis);

                    try
                    {
                        string formattedClassTime = classEntity.ClassTime.ToString("HH:mm");
                        string formattedTestDay = classEntity.TestDay.ToString("dd/MM/yyyy");

                        string notificationMessage =
                            "<p>Chào bạn,</p>" +
                            "<p>Cảm ơn bạn đã đăng ký tham gia lớp học " + classEntity.ClassName + " tại InstruLearn.</p>" +
                            "<p>Để hoàn tất việc xếp lớp, bạn vui lòng đến trung tâm InstruLearn vào lúc " + formattedClassTime + " ngày " + formattedTestDay + " để thực hiện kiểm tra chất lượng đầu vào.</p>" +
                            "<p>Việc kiểm tra này giúp chúng tôi sắp xếp lớp phù hợp với trình độ hiện tại của bạn và thực hiện thanh toán.</p>" +
                            "<p>Học viên sẽ thực hiện thanh toán phần học phí còn lại tại trung tâm. Nếu học viên không nộp học phí, thì học viên sẽ bị loại ra khỏi lớp.</p>" +
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

                        _logger.LogInformation($"Đã tạo thông báo kiểm tra đầu vào cho học viên {paymentDTO.LearnerId} cho lớp {paymentDTO.ClassId}");
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, $"Không thể tạo thông báo kiểm tra đầu vào cho học viên {paymentDTO.LearnerId}");
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation($"Học viên {paymentDTO.LearnerId} đã đăng ký thành công vào lớp {paymentDTO.ClassId} với thanh toán {paymentAmount} (10% tổng số {totalClassPrice})");

                    string certificateStatus;
                    if (classEntity.StartDate == today)
                    {
                        certificateStatus = "Đã tạo chứng chỉ tạm thời";
                    }
                    else if (classEntity.StartDate < today)
                    {
                        certificateStatus = "Sẽ được đánh giá dựa trên sự tham dự";
                    }
                    else
                    {
                        certificateStatus = $"Đã lên lịch cho {classEntity.StartDate}";
                    }

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Bạn đã đăng ký thành công vào lớp '{classEntity.ClassName}'. Thanh toán {paymentAmount} (10% tổng số {totalClassPrice}) đã được xử lý.",
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
                    _logger.LogError(ex, "Lỗi chi tiết trong quá trình đăng ký lớp học: {Message}", ex.Message);
                    if (ex.InnerException != null)
                    {
                        _logger.LogError("Lỗi bên trong: {Message}", ex.InnerException.Message);
                    }

                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình xử lý đăng ký lớp học với thanh toán");

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không thể đăng ký vào lớp học: {ex.Message}",
                    Data = null
                };
            }
        }

        private async Task CreateLearnerSchedulesForClass(int learnerId, int classId, Class classEntity, Learning_Registration learningRegis)
        {
            var existingLearnerSchedules = await _unitOfWork.ScheduleRepository.GetQuery()
                .Where(s => s.ClassId == classId &&
                           s.TeacherId == classEntity.TeacherId &&
                           s.LearnerId != null)
                .OrderBy(s => s.StartDay)
                .ToListAsync();

            if (existingLearnerSchedules != null && existingLearnerSchedules.Any())
            {
                _logger.LogInformation($"Đã tìm thấy {existingLearnerSchedules.Count} lịch trình hiện có cho các học viên khác trong lớp {classId}");

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

            var existingTeacherSchedules = await _unitOfWork.ScheduleRepository.GetQuery()
                .Where(s => s.ClassId == classId &&
                           s.TeacherId == classEntity.TeacherId &&
                           s.LearnerId == null)
                .OrderBy(s => s.StartDay)
                .ToListAsync();

            if (existingTeacherSchedules != null && existingTeacherSchedules.Any())
            {
                _logger.LogInformation($"Đã tìm thấy {existingTeacherSchedules.Count} lịch trình giáo viên hiện có cho lớp {classId}");

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

            _logger.LogWarning($"Không tìm thấy lịch trình hiện có cho lớp {classId}, đang tạo lịch trình mới");

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
                _logger.LogWarning($"Không tìm thấy ngày học cho lớp {classId}. Việc đăng ký có thể không hoàn chỉnh.");
            }
        }

        public async Task<ResponseDTO> RejectLearningRegisAsync(int learningRegisId, int? responseId)
        {
            try
            {
                _logger.LogInformation($"Bắt đầu quá trình từ chối đăng ký học tập cho đăng ký ID: {learningRegisId}");

                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    _logger.LogWarning($"Không tìm thấy đăng ký học tập với ID {learningRegisId}");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy đăng ký học tập."
                    };
                }

                if (learningRegis.Status != LearningRegis.Pending)
                {
                    _logger.LogWarning($"Không thể từ chối đăng ký {learningRegisId} với trạng thái {learningRegis.Status}. Chỉ những đăng ký đang chờ xử lý mới có thể bị từ chối.");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Không thể từ chối đăng ký với trạng thái {learningRegis.Status}. Chỉ những đăng ký đang chờ xử lý mới có thể bị từ chối."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    learningRegis.Status = LearningRegis.Rejected;
                    string responseDescription = "Không có lý do cụ thể.";
                    string responseTypeName = "Khác";

                    if (responseId.HasValue)
                    {
                        var response = await _unitOfWork.ResponseRepository.GetWithIncludesAsync(
                            r => r.ResponseId == responseId.Value,
                            "ResponseType");

                        if (response == null || !response.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Không tìm thấy phản hồi với ID {responseId.Value}."
                            };
                        }

                        var selectedResponse = response.First();
                        learningRegis.ResponseId = responseId.Value;
                        responseDescription = selectedResponse.ResponseName ?? responseDescription;

                        if (selectedResponse.ResponseType != null)
                        {
                            responseTypeName = selectedResponse.ResponseType.ResponseTypeName;
                        }
                    }

                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learningRegis.LearnerId);
                    if (learner != null)
                    {
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

                                await _emailService.SendEmailAsync(account.Email, subject, body, true);
                                _logger.LogInformation($"Đã gửi email từ chối đến {account.Email} cho đăng ký học tập {learningRegisId}");
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, $"Lỗi khi gửi email từ chối cho đăng ký học tập {learningRegisId}");
                            }
                        }
                    }
                    await _unitOfWork.SaveChangeAsync();


                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation($"Đăng ký học tập {learningRegisId} đã bị từ chối thành công");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Từ chối đăng ký học tập thành công.",
                        Data = new
                        {
                            LearningRegisId = learningRegisId,
                            ResponseId = learningRegis.ResponseId
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi trong quá trình từ chối đăng ký học tập {learningRegisId}: {ex.Message}");
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Đã xảy ra lỗi trong quá trình xử lý từ chối đăng ký học tập: {ex.Message}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không thể từ chối đăng ký học tập: {ex.Message}"
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
                        Message = "Không tìm thấy đăng ký học tập."
                    };
                }

                if (createDTO.LearningPathSessions.Count > learningRegis.NumberOfSession)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Số lượng buổi học trong lộ trình ({createDTO.LearningPathSessions.Count}) vượt quá số buổi học đã đăng ký ({learningRegis.NumberOfSession})."
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
                        Message = "Phát hiện số thứ tự buổi học bị trùng lặp trong yêu cầu."
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
                    Message = $"Đã tạo thành công các buổi học trong lộ trình cho đăng ký học tập {createDTO.LearningRegisId}.",
                    Data = createDTO.LearningPathSessions.Count
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không thể tạo buổi học trong lộ trình. " + ex.Message
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
                    _logger.LogInformation($"Trạng thái đăng ký học là {learningRegis.Status}. Đặt thanh toán đợt đầu là đã hoàn thành.");
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
                            _logger.LogInformation($"Đã tìm thấy ngày thanh toán: {firstPaymentDate} cho khoản thanh toán 40% của đăng ký {learningRegisId}");
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
                _logger.LogError(ex, $"Lỗi khi lấy thông tin giai đoạn thanh toán đầu tiên cho đăng ký {learningRegisId}");
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
                    _logger.LogInformation($"Trạng thái đăng ký học là {learningRegis.Status}. Đặt thanh toán đợt hai là đã hoàn thành.");
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
                                _logger.LogInformation($"Đã tìm thấy ngày thanh toán: {secondPaymentDate} cho khoản thanh toán 60% của đăng ký {learningRegisId}");
                            }
                        }
                    }
                }

                int? secondPaymentRemainingDays = null;
                DateTime? secondPaymentDeadline = null;

                bool isInSecondPaymentPhase =
                    (learningRegis.Status == LearningRegis.FourtyFeedbackDone ||
                     learningRegis.Status == LearningRegis.Fourty) &&
                    !secondPaymentCompleted;

                if (isInSecondPaymentPhase)
                {
                    if (learningRegis.ChangeTeacherRequested && !learningRegis.TeacherChangeProcessed)
                    {
                        secondPaymentStatus = "Đang chờ thay đổi giáo viên";
                        _logger.LogInformation($"Đăng ký học tập {learningRegisId} đang chờ thay đổi giáo viên");
                    }

                    if (learningRegis.PaymentDeadline.HasValue)
                    {
                        secondPaymentDeadline = learningRegis.PaymentDeadline;
                        _logger.LogInformation($"Sử dụng thời hạn thanh toán hiện có: {secondPaymentDeadline} cho đăng ký {learningRegisId}");
                    }

                    else
                    {
                        secondPaymentDeadline = DateTime.Now.AddDays(7);
                        _logger.LogInformation($"Tạo thời hạn thanh toán mới: {secondPaymentDeadline} cho đăng ký {learningRegisId} vì không có thời hạn");
                    }

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
                _logger.LogError(ex, $"Lỗi khi lấy thông tin giai đoạn thanh toán thứ hai cho đăng ký {learningRegisId}");
                return null;
            }
        }
    }
}