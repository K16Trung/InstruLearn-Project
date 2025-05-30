using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Class;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO.LearnerClass;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILearnerNotificationService _learnerNotificationService;
        private readonly IScheduleService _scheduleService;

        public ClassService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILearnerNotificationService learnerNotificationService,
            IScheduleService scheduleService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _learnerNotificationService = learnerNotificationService;
            _scheduleService = scheduleService;
        }

        public async Task<List<ClassDTO>> GetAllClassAsync()
        {
            var classes = await _unitOfWork.ClassRepository.GetAllAsync();

            var classDTOs = _mapper.Map<List<ClassDTO>>(classes);

            foreach (var classDTO in classDTOs)
            {
                if (string.IsNullOrEmpty(classDTO.MajorName))
                {
                    classDTO.MajorName = "Chưa được gán";
                }

                if (string.IsNullOrEmpty(classDTO.LevelName))
                {
                    classDTO.LevelName = "Chưa được gán";
                }

                // Set the SyllabusLink from the Level association
                if (string.IsNullOrEmpty(classDTO.SyllabusLink))
                {
                    var level = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(classDTO.LevelId);
                    classDTO.SyllabusLink = level?.SyllabusLink;
                }

                var classDayPatterns = await _unitOfWork.ClassDayRepository.GetQuery()
                    .Where(cd => cd.ClassId == classDTO.ClassId)
                    .ToListAsync();

                classDTO.ClassDays = _mapper.Map<List<ClassDayDTO>>(classDayPatterns);

                var sessionDates = new List<DateOnly>();
                DateOnly currentDate = classDTO.StartDate;
                int daysAdded = 0;

                var classMeetingDays = classDayPatterns.Select(cd => cd.Day).ToList();

                while (daysAdded < classDTO.totalDays)
                {
                    if (classMeetingDays.Contains((DayOfWeeks)currentDate.DayOfWeek))
                    {
                        sessionDates.Add(currentDate);
                        daysAdded++;
                    }

                    currentDate = currentDate.AddDays(1);
                }

                classDTO.SessionDates = sessionDates;
            }

            return classDTOs;
        }

        public async Task<ResponseDTO> GetClassByIdAsync(int id)
        {
            try
            {
                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(id);
                if (classEntity == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy lớp."
                    };
                }

                var classDays = await _unitOfWork.ClassDayRepository.GetQuery()
                    .Where(cd => cd.ClassId == id)
                    .ToListAsync();

                var classDetailDTO = _mapper.Map<ClassDetailDTO>(classEntity);

                classDetailDTO.ClassEndTime = classDetailDTO.ClassTime.AddHours(2);

                if (string.IsNullOrEmpty(classDetailDTO.SyllabusLink))
                {
                    var level = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(classDetailDTO.LevelId);
                    classDetailDTO.SyllabusLink = level?.SyllabusLink;
                }

                classDetailDTO.ClassDays = _mapper.Map<List<ClassDayDTO>>(classDays);

                var sessionDates = new List<DateOnly>();
                DateOnly currentDate = classDetailDTO.StartDate;
                int daysAdded = 0;
                var classMeetingDays = classDays.Select(cd => cd.Day).ToList();

                while (daysAdded < classDetailDTO.TotalDays)
                {
                    if (classMeetingDays.Contains((DayOfWeeks)currentDate.DayOfWeek))
                    {
                        sessionDates.Add(currentDate);
                        daysAdded++;
                    }
                    currentDate = currentDate.AddDays(1);
                }

                classDetailDTO.SessionDates = sessionDates;

                var studentCount = await _unitOfWork.dbContext.Learner_Classes
                    .Where(lc => lc.ClassId == id)
                    .CountAsync();

                classDetailDTO.StudentCount = studentCount;

                if (studentCount > 0)
                {
                    classDetailDTO.Students = await _unitOfWork.ClassRepository.GetClassStudentsWithEligibilityAsync(id);
                }
                else
                {
                    classDetailDTO.Students = new List<ClassStudentDTO>();
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy thông tin chi tiết về lớp học thành công.",
                    Data = classDetailDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không thể lấy thông tin chi tiết về lớp học: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetClassesByTeacherIdAsync(int teacherId)
        {
            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy giáo viên.",
                        Data = null
                    };
                }

                var classes = await _unitOfWork.ClassRepository.GetClassesByTeacherIdAsync(teacherId);

                if (classes == null || !classes.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Không tìm thấy lớp học nào của giáo viên này.",
                        Data = new List<ClassDTO>()
                    };
                }

                var classDTOs = _mapper.Map<List<ClassDTO>>(classes);

                foreach (var classDTO in classDTOs)
                {
                    if (string.IsNullOrEmpty(classDTO.MajorName))
                    {
                        classDTO.MajorName = "Chưa được gán";
                    }

                    if (string.IsNullOrEmpty(classDTO.LevelName))
                    {
                        classDTO.LevelName = "Chưa được gán";
                    }

                    // Set the SyllabusLink from the Level association
                    if (string.IsNullOrEmpty(classDTO.SyllabusLink))
                    {
                        var level = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(classDTO.LevelId);
                        classDTO.SyllabusLink = level?.SyllabusLink;
                    }

                    var classDayPatterns = await _unitOfWork.ClassDayRepository.GetQuery()
                        .Where(cd => cd.ClassId == classDTO.ClassId)
                        .ToListAsync();

                    classDTO.ClassDays = _mapper.Map<List<ClassDayDTO>>(classDayPatterns);

                    var sessionDates = new List<DateOnly>();
                    DateOnly currentDate = classDTO.StartDate;
                    int daysAdded = 0;

                    var classMeetingDays = classDayPatterns.Select(cd => cd.Day).ToList();

                    while (daysAdded < classDTO.totalDays)
                    {
                        if (classMeetingDays.Contains((DayOfWeeks)currentDate.DayOfWeek))
                        {
                            sessionDates.Add(currentDate);
                            daysAdded++;
                        }

                        currentDate = currentDate.AddDays(1);
                    }

                    classDTO.SessionDates = sessionDates;

                    var studentCount = await _unitOfWork.dbContext.Learner_Classes
                        .Where(lc => lc.ClassId == classDTO.ClassId)
                        .CountAsync();

                    classDTO.StudentCount = studentCount;

                    if (studentCount > 0)
                    {
                        classDTO.Students = await _unitOfWork.ClassRepository.GetClassStudentsWithEligibilityAsync(classDTO.ClassId);
                    }
                    else
                    {
                        classDTO.Students = new List<ClassStudentDTO>();
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã lấy {classDTOs.Count} lớp học của giáo viên ID: {teacherId}.",
                    Data = classDTOs
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy danh sách lớp học của giáo viên: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO> GetClassesByMajorIdAsync(int majorId)
        {
            var classes = await _unitOfWork.ClassRepository.GetClassesByMajorIdAsync(majorId);

            if (classes == null || !classes.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy lớp học nào cho gói khóa học đã cho.",
                    Data = null
                };
            }

            var classDtos = _mapper.Map<List<ClassDTO>>(classes);

            // Set the SyllabusLink for each class
            foreach (var classDto in classDtos)
            {
                if (string.IsNullOrEmpty(classDto.SyllabusLink))
                {
                    var level = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(classDto.LevelId);
                    classDto.SyllabusLink = level?.SyllabusLink;
                }
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã lấy danh sách lớp học thành công.",
                Data = classDtos
            };
        }

        public async Task<ResponseDTO> ChangeClassForLearnerAsync(ChangeClassDTO changeClassDTO)
        {
            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(changeClassDTO.LearnerId);
            if (learner == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy học viên."
                };
            }

            var currentLearnerClass = await _unitOfWork.dbContext.Learner_Classes
                .FirstOrDefaultAsync(lc => lc.LearnerId == changeClassDTO.LearnerId);

            if (currentLearnerClass == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Học viên chưa đăng ký lớp học nào."
                };
            }

            if (currentLearnerClass.ClassId == changeClassDTO.ClassId)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Học viên đã thuộc lớp này rồi."
                };
            }

            var currentClass = await _unitOfWork.ClassRepository.GetByIdAsync(currentLearnerClass.ClassId);
            if (currentClass == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy thông tin lớp học hiện tại."
                };
            }

            var newClass = await _unitOfWork.ClassRepository.GetByIdAsync(changeClassDTO.ClassId);
            if (newClass == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy lớp học mới."
                };
            }

            var studentCount = await _unitOfWork.dbContext.Learner_Classes
                .Where(lc => lc.ClassId == changeClassDTO.ClassId)
                .CountAsync();
            if (studentCount >= newClass.MaxStudents)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Lớp học mới đã đầy."
                };
            }

            var currentRegistration = await _unitOfWork.dbContext.Learning_Registrations
                .FirstOrDefaultAsync(lr => lr.LearnerId == changeClassDTO.LearnerId &&
                                          lr.ClassId == currentClass.ClassId);

            if (currentRegistration == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy thông tin đăng ký học hiện tại."
                };
            }

            decimal currentClassPrice = currentClass.Price * currentClass.totalDays;
            decimal newClassPrice = newClass.Price * newClass.totalDays;
            decimal initialPaymentMade = currentClassPrice * 0.1m;
            decimal remainingAmount = newClassPrice - initialPaymentMade;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                currentLearnerClass.ClassId = changeClassDTO.ClassId;
                await _unitOfWork.SaveChangeAsync();

                currentRegistration.ClassId = changeClassDTO.ClassId;
                currentRegistration.TeacherId = newClass.TeacherId;
                currentRegistration.RemainingAmount = remainingAmount;

                currentRegistration.Status = LearningRegis.FullyPaid;
                await _unitOfWork.LearningRegisRepository.UpdateAsync(currentRegistration);

                var schedules = await _unitOfWork.dbContext.Schedules
                    .Where(s => s.LearnerId == changeClassDTO.LearnerId && s.ClassId == currentClass.ClassId)
                    .ToListAsync();

                var newClassDays = await _unitOfWork.ClassDayRepository.GetQuery()
                    .Where(cd => cd.ClassId == newClass.ClassId)
                    .ToListAsync();

                var currentDate = DateOnly.FromDateTime(DateTime.Now);
                var futureSchedules = schedules.Where(s => s.StartDay >= currentDate).ToList();
                int updatedSchedules = 0;

                if (futureSchedules.Any() && newClassDays.Any())
                {
                    Dictionary<DateOnly, DateOnly> dateMapping = CreateDateMapping(
                        futureSchedules.Select(s => s.StartDay).OrderBy(d => d).ToList(),
                        newClass.StartDate,
                        newClassDays.Select(cd => cd.Day).ToList()
                    );

                    foreach (var schedule in futureSchedules)
                    {
                        schedule.ClassId = changeClassDTO.ClassId;
                        schedule.TeacherId = newClass.TeacherId;
                        schedule.TimeStart = newClass.ClassTime;
                        schedule.TimeEnd = newClass.ClassTime.AddHours(2);

                        if (dateMapping.TryGetValue(schedule.StartDay, out var newStartDay))
                        {
                            schedule.StartDay = newStartDay;
                        }

                        schedule.ChangeReason = !string.IsNullOrEmpty(changeClassDTO.Reason) ?
                            changeClassDTO.Reason :
                            $"Chuyển từ lớp {currentClass.ClassName} sang lớp {newClass.ClassName}";

                        await _unitOfWork.ScheduleRepository.UpdateAsync(schedule);
                        updatedSchedules++;
                    }
                }

                var notificationMessage = $"Bạn đã được chuyển từ lớp {currentClass.ClassName} sang lớp {newClass.ClassName}. " +
                                         $"Số tiền bạn đã thanh toán ({initialPaymentMade:N0} VND) đã được tính vào học phí mới. " +
                                         $"Vui lòng thanh toán số tiền còn lại {remainingAmount:N0} VND để tiếp tục học tập.";

                if (!string.IsNullOrEmpty(changeClassDTO.Reason))
                {
                    notificationMessage += $" Lý do chuyển lớp: {changeClassDTO.Reason}";
                }

                var notification = new StaffNotification
                {
                    LearnerId = changeClassDTO.LearnerId,
                    Title = "Thay đổi lớp học và thông tin thanh toán",
                    Message = notificationMessage,
                    Type = NotificationType.ClassChange,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangeAsync();

                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã chuyển lớp thành công.",
                    Data = new
                    {
                        LearnerId = changeClassDTO.LearnerId,
                        LearnerName = learner.FullName,
                        OldClassName = currentClass.ClassName,
                        NewClassName = newClass.ClassName,
                        InitialPaymentMade = initialPaymentMade,
                        RemainingAmount = remainingAmount,
                        TotalNewClassPrice = newClassPrice,
                        UpdatedSchedules = updatedSchedules,
                        IsEligible = true
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi chuyển lớp: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> AddClassAsync(CreateClassDTO createClassDTO)
        {

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            if (createClassDTO.TestDay == today)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không thể tạo lớp có ngày kiểm tra vào ngày hôm nay. Các học viên cần thời gian để chuẩn bị cho bài kiểm tra đầu vào."
                };
            }

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(createClassDTO.TeacherId);
            if (teacher == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy giáo viên",
                };
            }

            var major = await _unitOfWork.MajorRepository.GetByIdAsync(createClassDTO.MajorId);
            if (major == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy gói học",
                };
            }

            var levelAssigned = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(createClassDTO.LevelId);
            if (levelAssigned == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy cấp độ học",
                };
            }

            if (levelAssigned.MajorId != createClassDTO.MajorId)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Cấp độ học không thuộc gói học đã chọn",
                };
            }

            if (string.IsNullOrEmpty(levelAssigned.SyllabusLink))
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Cấp độ học này chưa có liên kết đến giáo trình",
                };
            }

            if (createClassDTO.ClassDays == null || !createClassDTO.ClassDays.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Lớp học phải có ít nhất một ngày học.",
                };
            }

            var classObj = _mapper.Map<Class>(createClassDTO);
            classObj.Teacher = teacher;
            classObj.Major = major;
            classObj.LevelId = createClassDTO.LevelId;

            DateOnly endDate = DateTimeHelper.CalculateEndDate(createClassDTO.StartDate, createClassDTO.totalDays, createClassDTO.ClassDays);

            if (today == createClassDTO.TestDay)
            {
                classObj.Status = ClassStatus.OnTestDay;
            }
            else if (createClassDTO.StartDate > today)
            {
                classObj.Status = ClassStatus.Scheduled;
            }
            else if (createClassDTO.StartDate <= today && endDate >= today)
            {
                classObj.Status = ClassStatus.Ongoing;
            }
            else
            {
                classObj.Status = ClassStatus.Completed;
            }

            classObj.ClassDays = createClassDTO.ClassDays.Select(day => new Model.Models.ClassDay
            {
                Day = day,
            }).ToList();

            await _unitOfWork.ClassRepository.AddAsync(classObj);
            await _unitOfWork.SaveChangeAsync();

            List<Schedules> teacherSchedules = new List<Schedules>();
            DateOnly currentDate = createClassDTO.StartDate;
            int classDaysCount = 0;

            while (classDaysCount < createClassDTO.totalDays)
            {
                if (createClassDTO.ClassDays.Contains((DayOfWeeks)currentDate.DayOfWeek))
                {
                    teacherSchedules.Add(new Schedules
                    {
                        TeacherId = teacher.TeacherId,
                        ClassId = classObj.ClassId,
                        StartDay = currentDate,
                        TimeStart = createClassDTO.ClassTime,
                        TimeEnd = createClassDTO.ClassTime.AddHours(2),
                        Mode = ScheduleMode.Center,
                        ScheduleDays = new List<ScheduleDays>
                        {
                            new ScheduleDays { DayOfWeeks = (DayOfWeeks)currentDate.DayOfWeek }
                        }
                    });
                    classDaysCount++;
                }
                currentDate = currentDate.AddDays(1);
            }

            await _unitOfWork.ScheduleRepository.AddRangeAsync(teacherSchedules);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã thêm lớp thành công",
                Data = new
                {
                    ClassId = classObj.ClassId,
                    StartDate = createClassDTO.StartDate,
                    EndDate = endDate,
                    TotalDays = createClassDTO.totalDays,
                    ClassDays = createClassDTO.ClassDays,
                    ScheduleCount = teacherSchedules.Count,
                    LevelId = classObj.LevelId,
                    LevelName = levelAssigned.LevelName,
                    SyllabusLink = levelAssigned.SyllabusLink
                }
            };
        }

        public async Task<ResponseDTO> UpdateClassAsync(int classId, UpdateClassDTO updateClassDTO)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
                if (classEntity == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy lớp!"
                    };
                }

                var previousStatus = classEntity.Status;

                classEntity.Status = updateClassDTO.Status;

                await _unitOfWork.ClassRepository.UpdateAsync(classEntity);
                await _unitOfWork.SaveChangeAsync();

                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã cập nhật trạng thái lớp thành công!",
                    Data = new
                    {
                        ClassId = classId,
                        ClassName = classEntity.ClassName,
                        PreviousStatus = previousStatus,
                        NewStatus = classEntity.Status
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật trạng thái lớp: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> DeleteClassAsync(int classId)
        {
            var deleteFeedback = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (deleteFeedback != null)
            {
                await _unitOfWork.ClassRepository.DeleteAsync(classId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã xóa lớp thành công"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy lớp có ID {classId}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateLearnerClassEligibilityAsync(LearnerClassEligibilityDTO eligibilityDTO)
        {
            try
            {
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(eligibilityDTO.LearnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(eligibilityDTO.ClassId);
                if (classEntity == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy lớp học."
                    };
                }

                DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
                if (currentDate > classEntity.TestDay)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không thể cập nhật trạng thái kiểm tra đầu vào sau khi ngày kiểm tra đã qua."
                    };
                }

                // Check if the learner is enrolled in this class
                var learnerClass = await _unitOfWork.dbContext.Learner_Classes
                    .FirstOrDefaultAsync(lc => lc.LearnerId == eligibilityDTO.LearnerId && lc.ClassId == eligibilityDTO.ClassId);

                if (learnerClass == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Học viên không thuộc lớp học này."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Find the learning registration for this learner and class
                    var learningRegis = await _unitOfWork.dbContext.Learning_Registrations
                        .FirstOrDefaultAsync(lr => lr.LearnerId == eligibilityDTO.LearnerId &&
                                                   lr.ClassId == eligibilityDTO.ClassId);

                    if (learningRegis != null)
                    {
                        // Simply update the status without trying to set TestResults
                        learningRegis.Status = eligibilityDTO.IsEligible ?
                            LearningRegis.FullyPaid : LearningRegis.TestFailed;

                        await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                    }

                    // Create notification based on eligibility
                    string notificationMessage;
                    NotificationType notificationType;
                    string notificationTitle;

                    if (eligibilityDTO.IsEligible)
                    {
                        notificationType = NotificationType.PaymentReminder;
                        notificationTitle = $"Đạt kiểm tra đầu vào - Lớp {classEntity.ClassName}";

                        decimal totalClassPrice = classEntity.Price * classEntity.totalDays;
                        decimal remainingPayment = Math.Round(totalClassPrice * 0.9m, 2);

                        notificationMessage = $"Chúc mừng {learner.FullName}!" +
                                             $"Bạn đã vượt qua bài kiểm tra đầu vào cho lớp {classEntity.ClassName}. " +
                                             $"Để hoàn tất quá trình đăng ký, vui lòng thanh toán số tiền còn lại {remainingPayment:N0} VND " +
                                             $"(90% học phí) tại trung tâm hoặc qua hệ thống thanh toán trực tuyến.</p>" +
                                             $"Sau khi thanh toán đầy đủ, bạn sẽ chính thức là học viên của lớp {classEntity.ClassName}.";
                    }
                    else
                    {
                        notificationType = NotificationType.ClassChange;
                        notificationTitle = $"Kết quả kiểm tra đầu vào - Lớp {classEntity.ClassName}";
                        notificationMessage = $"Kính gửi {learner.FullName},</p>" +
                                             $"Cảm ơn bạn đã tham gia bài kiểm tra đầu vào cho lớp {classEntity.ClassName}. " +
                                             $"Dựa trên kết quả, chúng tôi nhận thấy lớp học này có thể chưa phù hợp với trình độ hiện tại của bạn." +
                                             $"Vui lòng liên hệ với nhân viên tư vấn để được hướng dẫn chuyển sang lớp phù hợp hơn.";
                    }

                    var notification = new StaffNotification
                    {
                        LearnerId = eligibilityDTO.LearnerId,
                        Title = notificationTitle,
                        Message = notificationMessage,
                        Type = notificationType,
                        Status = NotificationStatus.Unread,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                    await _unitOfWork.SaveChangeAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = eligibilityDTO.IsEligible ?
                            "Cập nhật trạng thái đạt kiểm tra đầu vào thành công." :
                            "Cập nhật trạng thái không đạt kiểm tra đầu vào thành công.",
                        Data = new
                        {
                            LearnerId = eligibilityDTO.LearnerId,
                            ClassId = eligibilityDTO.ClassId,
                            IsEligible = eligibilityDTO.IsEligible,
                        }
                    };
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Lỗi khi cập nhật trạng thái kiểm tra đầu vào: {ex.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> RemoveLearnerFromClassAsync(RemoveLearnerFromClassDTO removalDTO)
        {
            try
            {
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(removalDTO.LearnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(removalDTO.ClassId);
                if (classEntity == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy lớp học."
                    };
                }

                // Check if the learner is actually enrolled in this class
                var learnerClass = await _unitOfWork.LearnerClassRepository.GetByLearnerAndClassAsync(
                    removalDTO.LearnerId, removalDTO.ClassId);

                if (learnerClass == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Học viên không thuộc lớp học này."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // 1. Remove from Learner_Classes table
                    await _unitOfWork.LearnerClassRepository.RemoveByLearnerAndClassAsync(
                        removalDTO.LearnerId, removalDTO.ClassId);
                    await _unitOfWork.SaveChangeAsync();

                    // 2. Delete Learning Registration records
                    var registrations = await _unitOfWork.LearningRegisRepository.GetQuery()
                        .Where(lr => lr.LearnerId == removalDTO.LearnerId &&
                                   lr.ClassId == removalDTO.ClassId)
                        .ToListAsync();

                    foreach (var registration in registrations)
                    {
                        await _unitOfWork.LearningRegisRepository.DeleteAsync(registration.LearningRegisId);
                    }
                    await _unitOfWork.SaveChangeAsync();

                    // 3. Remove associated schedules
                    var schedules = await _unitOfWork.ScheduleRepository.GetQuery()
                        .Where(s => s.LearnerId == removalDTO.LearnerId &&
                                  s.ClassId == removalDTO.ClassId)
                        .ToListAsync();

                    foreach (var schedule in schedules)
                    {
                        await _unitOfWork.ScheduleRepository.DeleteAsync(schedule.ScheduleId);
                    }
                    await _unitOfWork.SaveChangeAsync();

                    // 4. Create notification for the learner
                    var notification = new StaffNotification
                    {
                        LearnerId = removalDTO.LearnerId,
                        Title = $"Thông báo xóa khỏi lớp {classEntity.ClassName}",
                        Message = $"Bạn đã bị xóa khỏi lớp {classEntity.ClassName}.",
                        Type = NotificationType.ClassChange,
                        Status = NotificationStatus.Unread,
                        CreatedAt = DateTime.Now
                    };

                    await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                    await _unitOfWork.SaveChangeAsync();

                    await _unitOfWork.CommitTransactionAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Học viên {learner.FullName} đã bị xóa khỏi lớp {classEntity.ClassName} thành công.",
                        Data = new
                        {
                            LearnerId = removalDTO.LearnerId,
                            LearnerName = learner.FullName,
                            ClassId = removalDTO.ClassId,
                            ClassName = classEntity.ClassName,
                            RemovalDate = DateTime.Now
                        }
                    };
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi xảy ra khi xóa học viên khỏi lớp học: {ex.Message}"
                };
            }
        }

        private Dictionary<DateOnly, DateOnly> CreateDateMapping(
    List<DateOnly> oldDates,
    DateOnly newClassStartDate,
    List<DayOfWeeks> newClassDays)
        {
            var result = new Dictionary<DateOnly, DateOnly>();
            if (!oldDates.Any() || !newClassDays.Any())
                return result;

            oldDates.Sort();

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            if (newClassStartDate < currentDate)
            {
                while (!newClassDays.Contains((DayOfWeeks)currentDate.DayOfWeek))
                {
                    currentDate = currentDate.AddDays(1);
                }
            }
            else
            {
                currentDate = newClassStartDate;
            }

            var newDates = new List<DateOnly>();
            while (newDates.Count < oldDates.Count)
            {
                if (newClassDays.Contains((DayOfWeeks)currentDate.DayOfWeek))
                {
                    newDates.Add(currentDate);
                }
                currentDate = currentDate.AddDays(1);
            }

            for (int i = 0; i < Math.Min(oldDates.Count, newDates.Count); i++)
            {
                result[oldDates[i]] = newDates[i];
            }

            return result;
        }
    }
}
