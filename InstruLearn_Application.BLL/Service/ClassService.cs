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

        public ClassService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ClassDTO>> GetAllClassAsync()
        {
            var classes = await _unitOfWork.ClassRepository.GetAllAsync();

            // Map classes to DTOs
            var classDTOs = _mapper.Map<List<ClassDTO>>(classes);

            foreach (var classDTO in classDTOs)
            {
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

                var syllabus = await _unitOfWork.SyllabusRepository.GetSyllabusByClassIdAsync(id);

                var classDetailDTO = _mapper.Map<ClassDetailDTO>(classEntity);

                if (syllabus != null)
                {
                    classDetailDTO.SyllabusId = syllabus.SyllabusId;
                    classDetailDTO.SyllabusName = syllabus.SyllabusName;
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

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Classes retrieved successfully.",
                Data = classDtos
            };
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

            var Syllabus = await _unitOfWork.SyllabusRepository.GetByIdAsync(createClassDTO.SyllabusId);
            if (Syllabus == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy giáo trình học",
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

            // Save schedules to database
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
                    ScheduleCount = teacherSchedules.Count
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
    }
}
