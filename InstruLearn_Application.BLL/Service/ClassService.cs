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
            var ClassGetAll = await _unitOfWork.ClassRepository.GetAllAsync();
            var ClassMapper = _mapper.Map<List<ClassDTO>>(ClassGetAll);
            return ClassMapper;
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
                        Message = "Class not found."
                    };
                }

                // Get class days
                var classDays = await _unitOfWork.ClassDayRepository.GetQuery()
                    .Where(cd => cd.ClassId == id)
                    .ToListAsync();

                // Get students enrolled in this class
                var learnerClasses = await _unitOfWork.dbContext.Learner_Classes
                    .Where(lc => lc.ClassId == id)
                    .Include(lc => lc.Learner)
                        .ThenInclude(l => l.Account)
                    .ToListAsync();

                // Map to DTO
                var classDetailDTO = _mapper.Map<ClassDetailDTO>(classEntity);

                // Map class days
                classDetailDTO.ClassDays = _mapper.Map<List<ClassDayDTO>>(classDays);

                // Add student information
                classDetailDTO.StudentCount = learnerClasses.Count;
                classDetailDTO.Students = learnerClasses.Select(lc => new ClassStudentDTO
                {
                    LearnerId = lc.LearnerId,
                    FullName = lc.Learner?.FullName ?? "N/A",
                    Email = lc.Learner?.Account?.Email ?? "N/A",
                    PhoneNumber = lc.Learner?.Account?.PhoneNumber ?? "N/A",
                    Avatar = lc.Learner?.Account?.Avatar ?? "N/A"

                }).ToList();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Class details retrieved successfully.",
                    Data = classDetailDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to retrieve class details: {ex.Message}"
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
                    Message = "No classes found for the given course package.",
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
            // Check if Teacher exists
            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(createClassDTO.TeacherId);
            if (teacher == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy giáo viên",
                };
            }

            // Check if CoursePackage exists
            var major = await _unitOfWork.MajorRepository.GetByIdAsync(createClassDTO.MajorId);
            if (major == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy gói học",
                };
            }
            // Check if CoursePackage exists
            var Syllabus = await _unitOfWork.SyllabusRepository.GetByIdAsync(createClassDTO.SyllabusId);
            if (Syllabus == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy giáo trình học",
                };
            }

            // Validate and add ClassDays (at least one day required)
            if (createClassDTO.ClassDays == null || !createClassDTO.ClassDays.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Lớp học phải có ít nhất một ngày học.",
                };
            }

            // Map CreateClassDTO to Class entity
            var classObj = _mapper.Map<Class>(createClassDTO);
            classObj.Teacher = teacher;
            classObj.Major = major;

            // Calculate the end date for the class
            DateOnly endDate = DateTimeHelper.CalculateEndDate(createClassDTO.StartDate, createClassDTO.totalDays, createClassDTO.ClassDays);

            // Determine class status based on the current date
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

            // Add ClassDays
            classObj.ClassDays = createClassDTO.ClassDays.Select(day => new Model.Models.ClassDay
            {
                Day = day,
            }).ToList();

            // Save the class in the database
            await _unitOfWork.ClassRepository.AddAsync(classObj);
            await _unitOfWork.SaveChangeAsync();

            // 🔹 Generate Schedules for Teacher 🔹
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
                        StartDay = currentDate,  // Use the actual current date for each schedule
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

                // Store the previous status
                var previousStatus = classEntity.Status;

                // Update the status property
                classEntity.Status = updateClassDTO.Status;

                await _unitOfWork.ClassRepository.UpdateAsync(classEntity);
                await _unitOfWork.SaveChangeAsync();

                // Commit the transaction regardless of the SaveChangeAsync return value
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
