using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Class;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using InstruLearn_Application.Model.Models.DTO.Feedback;
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

        public ClassService(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            ILearnerNotificationService learnerNotificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _learnerNotificationService = learnerNotificationService;
        }

        public async Task<List<ClassDTO>> GetAllClassAsync()
        {
            var classes = await _unitOfWork.ClassRepository.GetAllAsync();

            var classDTOs = _mapper.Map<List<ClassDTO>>(classes);

            foreach (var classDTO in classDTOs)
            {
                if (string.IsNullOrEmpty(classDTO.MajorName))
                {
                    classDTO.MajorName = "Not Assigned";
                }

                if (string.IsNullOrEmpty(classDTO.LevelName))
                {
                    classDTO.LevelName = "Not Assigned";
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
                    var students = await _unitOfWork.dbContext.Learner_Classes
                        .Where(lc => lc.ClassId == id)
                        .Join(
                            _unitOfWork.dbContext.Learners,
                            lc => lc.LearnerId,
                            l => l.LearnerId,
                            (lc, l) => new { LearnerId = l.LearnerId, Learner = l }
                        )
                        .Join(
                            _unitOfWork.dbContext.Accounts,
                            join => join.Learner.AccountId,
                            a => a.AccountId,
                            (join, a) => new ClassStudentDTO
                            {
                                LearnerId = join.LearnerId,
                                FullName = join.Learner.FullName ?? "N/A",
                                Email = a.Email ?? "N/A",
                                PhoneNumber = a.PhoneNumber ?? "N/A",
                                Avatar = a.Avatar ?? "N/A"
                            }
                        )
                        .ToListAsync();

                    classDetailDTO.Students = students;
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
                        Message = "Teacher not found.",
                        Data = null
                    };
                }

                var classes = await _unitOfWork.ClassRepository.GetClassesByTeacherIdAsync(teacherId);

                if (classes == null || !classes.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No classes found for this teacher.",
                        Data = new List<ClassDTO>()
                    };
                }

                var classDTOs = _mapper.Map<List<ClassDTO>>(classes);

                foreach (var classDTO in classDTOs)
                {
                    if (string.IsNullOrEmpty(classDTO.MajorName))
                    {
                        classDTO.MajorName = "Not Assigned";
                    }

                    if (string.IsNullOrEmpty(classDTO.LevelName))
                    {
                        classDTO.LevelName = "Not Assigned";
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
                        var students = await _unitOfWork.dbContext.Learner_Classes
                            .Where(lc => lc.ClassId == classDTO.ClassId)
                            .Join(
                                _unitOfWork.dbContext.Learners,
                                lc => lc.LearnerId,
                                l => l.LearnerId,
                                (lc, l) => new { LearnerId = l.LearnerId, Learner = l }
                            )
                            .Join(
                                _unitOfWork.dbContext.Accounts,
                                join => join.Learner.AccountId,
                                a => a.AccountId,
                                (join, a) => new ClassStudentDTO
                                {
                                    LearnerId = join.LearnerId,
                                    FullName = join.Learner.FullName ?? "N/A",
                                    Email = a.Email ?? "N/A",
                                    PhoneNumber = a.PhoneNumber ?? "N/A",
                                    Avatar = a.Avatar ?? "N/A"
                                }
                            )
                            .ToListAsync();

                        classDTO.Students = students;
                    }
                    else
                    {
                        classDTO.Students = new List<ClassStudentDTO>();
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {classDTOs.Count} classes for teacher ID: {teacherId}.",
                    Data = classDTOs
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving classes for teacher: {ex.Message}",
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
                Message = "Classes retrieved successfully.",
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

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                currentLearnerClass.ClassId = changeClassDTO.ClassId;
                await _unitOfWork.SaveChangeAsync();

                var learningRegistrations = await _unitOfWork.dbContext.Learning_Registrations
                    .Where(lr => lr.LearnerId == changeClassDTO.LearnerId && lr.ClassId == currentClass.ClassId)
                    .ToListAsync();

                foreach (var registration in learningRegistrations)
                {
                    registration.ClassId = changeClassDTO.ClassId;
                    registration.TeacherId = newClass.TeacherId;
                    await _unitOfWork.LearningRegisRepository.UpdateAsync(registration);
                }

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

                var notificationMessage = $"Bạn đã được chuyển từ lớp {currentClass.ClassName} sang lớp {newClass.ClassName}.";
                if (!string.IsNullOrEmpty(changeClassDTO.Reason))
                {
                    notificationMessage += $" Lý do: {changeClassDTO.Reason}";
                }

                var notification = new StaffNotification
                {
                    LearnerId = changeClassDTO.LearnerId,
                    Title = "Thay đổi lớp học",
                    Message = notificationMessage,
                    Type = NotificationType.ClassChange,
                    Status = NotificationStatus.Unread,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.StaffNotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangeAsync();

                // Commit transaction
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
                        UpdatedLearningRegistrations = learningRegistrations.Count,
                        UpdatedSchedules = updatedSchedules
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

            DateTime now = DateTime.Now;
            if (createClassDTO.StartDate.ToDateTime(new TimeOnly(0, 0)) > now)
            {
                classObj.Status = ClassStatus.Scheduled;
            }
            else if (createClassDTO.StartDate.ToDateTime(new TimeOnly(0, 0)) <= now &&
                     endDate.ToDateTime(new TimeOnly(23, 59)) >= now)
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
