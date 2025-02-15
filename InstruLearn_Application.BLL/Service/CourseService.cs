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

        public async Task<ResponseDTO> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllWithTypeAsync();
            var courseDtos = _mapper.Map<IEnumerable<CourseDTO>>(courses);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Courses retrieved successfully.",
                Data = courseDtos
            };
        }

        public async Task<ResponseDTO> GetCourseByIdAsync(int courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdWithTypeAsync(courseId);
            if (course == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course not found."
                };
            }
            var courseDto = _mapper.Map<CourseDTO>(course);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course retrieved successfully.",
                Data = courseDto
            };
        }

        public async Task<ResponseDTO> AddCourseAsync(CreateCourseDTO createDto)
        {
            var course = _mapper.Map<Course>(createDto);
            await _unitOfWork.CourseRepository.AddAsync(course);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course added successfully.",
            };
        }

        public async Task<ResponseDTO> UpdateCourseAsync(int courseId, UpdateCourseDTO updateDto)
        {
            var existingCourse = await _unitOfWork.CourseRepository.GetByIdWithTypeAsync(courseId);
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
            var course = await _unitOfWork.CourseRepository.GetByIdWithTypeAsync(courseId);
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
    }

}
