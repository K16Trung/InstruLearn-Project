using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Class;
using InstruLearn_Application.Model.Models.DTO.Feedback;
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

        public async Task<ClassDTO> GetClassByIdAsync(int classId)
        {
            var ClassGetById = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            var ClassMapper = _mapper.Map<ClassDTO>(ClassGetById);
            return ClassMapper;
        }

        public async Task<ResponseDTO> GetClassesByCoursePackageIdAsync(int coursePackageId)
        {
            var classes = await _unitOfWork.ClassRepository.GetClassesByCoursePackageIdAsync(coursePackageId);

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
            var coursePackage = await _unitOfWork.CourseRepository.GetByIdAsync(createClassDTO.CoursePackageId);
            if (coursePackage == null)
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
            classObj.CoursePackage = coursePackage;

            // Determine class status based on the current date
            DateTime now = DateTime.Now;
            if (createClassDTO.StartDate.ToDateTime(new TimeOnly(0, 0)) > now)
            {
                classObj.Status = ClassStatus.Scheduled;
            }
            else if (createClassDTO.StartDate.ToDateTime(new TimeOnly(0, 0)) <= now &&
                     createClassDTO.StartDate.AddDays(createClassDTO.totalDays - 1).ToDateTime(new TimeOnly(23, 59)) >= now)
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
                Day = day,  // Assuming Day is stored as an enum in your ClassDay model
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
                        ClassId = classObj.ClassId,  // Assign newly created class ID
                        TimeStart = createClassDTO.ClassTime,
                        TimeEnd = createClassDTO.ClassTime.AddHours(2),  // Assuming class duration is 2 hours
                        Mode = ScheduleMode.Center,  // Change as needed
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
            };
        }

        public async Task<ResponseDTO> UpdateClassAsync(int classId, UpdateClassDTO updateClassDTO)
        {
            var classUpdate = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (classUpdate != null)
            {
                classUpdate = _mapper.Map(updateClassDTO, classUpdate);
                await _unitOfWork.ClassRepository.UpdateAsync(classUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Đã cập nhật lớp thành công!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Cập nhật lớp thất bại!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Không tìm thấy lớp!"
            };
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
