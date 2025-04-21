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
                Message = "Đã truy xuất nội dung khóa học thành công.",
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
                    Message = "Không tìm thấy nội dung khóa học."
                };
            }
            var courseContentDtos = _mapper.Map<CourseContentDTO>(courseContent);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Nội dung khóa học đã được truy xuất thành công.",
                Data = courseContentDtos
            };
        }

        public async Task<ResponseDTO> AddCourseContentAsync(CreateCourseContentDTO createDto)
        {
            try
            {
                var courseContent = _mapper.Map<Course_Content>(createDto);
                await _unitOfWork.CourseContentRepository.AddAsync(courseContent);
                await _unitOfWork.SaveChangeAsync();

                await _unitOfWork.LearnerCourseRepository.RecalculateProgressForAllLearnersInCourseAsync(createDto.CoursePackageId);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Khóa học đã được thêm thành công.",
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi thêm khóa học: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateCourseContentAsync(int courseContentId, UpdateCourseContentDTO updateDto)
        {
            var existingCourseContent = await _unitOfWork.CourseContentRepository.GetByIdAsync(courseContentId);
            if (existingCourseContent == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy nội dung khóa học."
                };
            }
            _mapper.Map(updateDto, existingCourseContent);
            await _unitOfWork.CourseContentRepository.UpdateAsync(existingCourseContent);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Nội dung khóa học đã được cập nhật thành công."
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
                    Message = "Không tìm thấy nội dung khóa học."
                };
            }
            await _unitOfWork.CourseContentRepository.DeleteAsync(courseContentId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Nội dung khóa học đã được xóa thành công."
            };
        }
        
    }
}
