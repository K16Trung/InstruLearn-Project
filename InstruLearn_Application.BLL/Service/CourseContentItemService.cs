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
                Message = "Course content items retrieved successfully.",
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
                    Message = "Course content item not found."
                };
            }
            var courseContentItemDto = _mapper.Map<CourseContentItemDTO>(courseContentItem);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course content item retrieved successfully.",
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
                Message = "Course content item added successfully."
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
                    Message = "Course content item not found."
                };
            }
            _mapper.Map(updateDto, existingItem);
            await _unitOfWork.CourseContentItemRepository.UpdateAsync(existingItem);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course content item updated successfully."
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
                    Message = "Course content item not found."
                };
            }
            await _unitOfWork.CourseContentItemRepository.DeleteAsync(itemId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Course content item deleted successfully."
            };
        }
    }



}
