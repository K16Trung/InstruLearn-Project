using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO.Course_Content_Item;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CourseContentItemService : ICourseContentItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseContentItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> GetAllCourseContentItemsAsync()
        {
            var courseContentItems = await _unitOfWork.CourseContentItemRepository.GetAllAsync();
            var courseContentItemDtos = _mapper.Map<IEnumerable<CourseContentItemDTO>>(courseContentItems);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã lấy ra danh sách nội dung khóa học thành công.",
                Data = courseContentItemDtos
            };
        }

        public async Task<ResponseDTO> GetCourseContentItemByIdAsync(int itemId)
        {
            var courseContentItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(itemId);
            if (courseContentItem == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy nội dung khóa học."
                };
            }
            var courseContentItemDto = _mapper.Map<CourseContentItemDTO>(courseContentItem);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã lấy ra danh sách nội dung khóa học thành công.",
                Data = courseContentItemDto
            };
        }

        public async Task<ResponseDTO> AddCourseContentItemAsync(CreateCourseContentItemDTO createDto)
        {
            var courseContentItem = _mapper.Map<Course_Content_Item>(createDto);
            await _unitOfWork.CourseContentItemRepository.AddAsync(courseContentItem);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã thêm nội dung khóa học thành công."
            };
        }

        public async Task<ResponseDTO> UpdateCourseContentItemAsync(int itemId, UpdateCourseContentItemDTO updateDto)
        {
            var existingItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(itemId);
            if (existingItem == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy nội dung khóa học."
                };
            }
            _mapper.Map(updateDto, existingItem);
            await _unitOfWork.CourseContentItemRepository.UpdateAsync(existingItem);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Nội dung khóa học đã được cập nhật thành công."
            };
        }

        public async Task<ResponseDTO> DeleteCourseContentItemAsync(int itemId)
        {
            var courseContentItem = await _unitOfWork.CourseContentItemRepository.GetByIdAsync(itemId);
            if (courseContentItem == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy nội dung khóa học."
                };
            }
            await _unitOfWork.CourseContentItemRepository.DeleteAsync(itemId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã xóa nội dung khóa học thành công."
            };
        }
    }



}
