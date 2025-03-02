using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Course_Content;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CourseContentService : ICourseContentService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseContentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> GetAllCourseContentAsync()
        {
            var coursesContent = await _unitOfWork.CourseContentRepository.GetAllAsync();
            var courseContentDtos = _mapper.Map<IEnumerable<CourseContentDTO>>(coursesContent);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Courses content retrieved successfully.",
                Data = courseContentDtos
            };
        }

        public async Task<ResponseDTO> GetCourseContentByIdAsync(int courseContentId)
        {
            var courseContent = await _unitOfWork.CourseContentRepository.GetByIdAsync(courseContentId);
            if (courseContent == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course content not found."
                };
            }
            var courseContentDtos = _mapper.Map<CourseContentDTO>(courseContent);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course content retrieved successfully.",
                Data = courseContentDtos
            };
        }

        public async Task<ResponseDTO> AddCourseContentAsync(CreateCourseContentDTO createDto)
        {
            var courseContent = _mapper.Map<Course_Content>(createDto);
            await _unitOfWork.CourseContentRepository.AddAsync(courseContent);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course added successfully.",
            };
        }

        public async Task<ResponseDTO> UpdateCourseContentAsync(int courseContentId, UpdateCourseContentDTO updateDto)
        {
            var existingCourseContent = await _unitOfWork.CourseContentRepository.GetByIdAsync(courseContentId);
            if (existingCourseContent == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course content not found."
                };
            }
            _mapper.Map(updateDto, existingCourseContent);
            await _unitOfWork.CourseContentRepository.UpdateAsync(existingCourseContent);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course content updated successfully."
            };
        }

        public async Task<ResponseDTO> DeleteCourseContentAsync(int courseContentId)
        {
            var courseContent = await _unitOfWork.CourseContentRepository.GetByIdAsync(courseContentId);
            if (courseContent == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course content not found."
                };
            }
            await _unitOfWork.CourseContentRepository.DeleteAsync(courseContentId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course content deleted successfully."
            };
        }
        
    }
}
