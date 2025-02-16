using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Course;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CourseTypeService : ICourseTypeService
    {

        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> GetAllCourseTypeAsync()
        {
            var coursesType = await _unitOfWork.CourseTypeRepository.GetAllAsync();
            var coursetypeDtos = _mapper.Map<IEnumerable<CourseTypeDTO>>(coursesType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Courses type retrieved successfully.",
                Data = coursetypeDtos
            };
        }

        public async Task<ResponseDTO> GetCourseTypeByIdAsync(int courseTypeId)
        {
            var courseType = await _unitOfWork.CourseTypeRepository.GetByIdAsync(courseTypeId);
            if (courseType == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course type not found."
                };
            }
            var courseTypeDto = _mapper.Map<CourseTypeDTO>(courseType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course type retrieved successfully.",
                Data = courseTypeDto
            };
        }


        public async Task<ResponseDTO> AddCourseTypeAsync(CreateCourseTypeDTO createDto)
        {
            var courseType = _mapper.Map<CourseType>(createDto);
            await _unitOfWork.CourseTypeRepository.AddAsync(courseType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course added successfully.",
            };
        }

        public async Task<ResponseDTO> UpdateCourseTypeAsync(int courseTypeId, UpdateCourseTypeDTO updateDto)
        {
            var existingCourseType = await _unitOfWork.CourseTypeRepository.GetByIdAsync(courseTypeId);
            if (existingCourseType == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course type not found."
                };
            }
            _mapper.Map(updateDto, existingCourseType);
            await _unitOfWork.CourseTypeRepository.UpdateAsync(existingCourseType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course type updated successfully."
            };
        }

        public async Task<ResponseDTO> DeleteCourseTypeAsync(int courseTypeId)
        {
            var courseType = await _unitOfWork.CourseTypeRepository.GetByIdAsync(courseTypeId);
            if (courseType == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course type not found."
                };
            }
            await _unitOfWork.CourseTypeRepository.DeleteAsync(courseTypeId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course type deleted successfully."
            };
        }
        
    }
}
