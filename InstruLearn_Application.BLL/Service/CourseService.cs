using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Models.DTO.Course;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Enum;

namespace InstruLearn_Application.BLL.Service
{
    public class CourseService : ICourseService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<GetAllCourseDTO>> GetAllCoursesAsync()
        {
            var CourseGetAll = await _unitOfWork.CourseRepository.GetAllAsync();
            var CourseMapper = _mapper.Map<List<GetAllCourseDTO>>(CourseGetAll);
            foreach (var course in CourseGetAll)
            {
                var courseDTO = CourseMapper.FirstOrDefault(c => c.CoursePackageId == course.CoursePackageId);
                if (courseDTO != null)
                {
                    var feedbacks = await _unitOfWork.FeedbackRepository.GetFeedbacksByCoursePackageIdAsync(course.CoursePackageId);
                    courseDTO.Rating = (int)CalculateAverageRating(feedbacks.ToList());
                }
            }
            return CourseMapper;
        }

        public async Task<CourseDTO> GetCourseByIdAsync(int courseId)
        {
            var courseGetById = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            var courseMapper = _mapper.Map<CourseDTO>(courseGetById);
            if (courseGetById != null && courseGetById.FeedBacks != null)
            {
                courseMapper.Rating = (int)CalculateAverageRating(courseGetById.FeedBacks);
            }
            return courseMapper;
        }

        // Get courses with status = 0
        public async Task<List<CoursePackageTypeDTO>> GetAllCoursesWithStatusZeroAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync();

            // Filter courses with status = 0 and map to DTO
            var filteredCourses = courses.Where(c => (int)c.Status == 0).ToList();
            return _mapper.Map<List<CoursePackageTypeDTO>>(filteredCourses);
        }

        // Get courses with status = 1
        public async Task<List<CoursePackageTypeDTO>> GetAllCoursesWithStatusOneAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync();

            // Filter courses with status = 1 and map to DTO
            var filteredCourses = courses.Where(c => (int)c.Status == 1).ToList();
            return _mapper.Map<List<CoursePackageTypeDTO>>(filteredCourses);
        }

        public async Task<ResponseDTO> AddCourseAsync(CreateCourseDTO createDto)
        {
            var type = await _unitOfWork.CourseTypeRepository.GetByIdAsync(createDto.CourseTypeId);
            if (type == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course Type not found",
                };
            }

            if (!Enum.IsDefined(typeof(CoursePackageStatus), createDto.Status))
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Invalid course package type.",
                };
            }

            var course = _mapper.Map<Course_Package>(createDto);
            course.Type = type;
            course.Status = createDto.Status;
            await _unitOfWork.CourseRepository.AddAsync(course);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course added successfully.",
            };
        }

        public async Task<ResponseDTO> UpdateCourseAsync(int courseId, UpdateCourseDTO updateDto)
        {
            var existingCourse = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (existingCourse == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course not found."
                };
            }
            _mapper.Map(updateDto, existingCourse);
            await _unitOfWork.CourseRepository.UpdateAsync(existingCourse);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course updated successfully."
            };
        }

        public async Task<ResponseDTO> DeleteCourseAsync(int courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course not found."
                };
            }
            await _unitOfWork.CourseRepository.DeleteAsync(courseId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course deleted successfully."
            };
        }
        private double CalculateAverageRating(ICollection<FeedBack> feedbacks)
        {
            if (feedbacks == null || feedbacks.Count == 0)
                return 0;

            double totalRating = feedbacks.Sum(f => f.Rating);
            return Math.Round(totalRating / feedbacks.Count, 1);
        }
    }
}
