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
            try
            {
                var courseContent = await _unitOfWork.CourseContentRepository.GetByIdAsync(createDto.ContentId);
                if (courseContent == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy nội dung khóa học với ID đã cung cấp."
                    };
                }

                var itemType = await _unitOfWork.ItemTypeRepository.GetByIdAsync(createDto.ItemTypeId);
                if (itemType == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy loại mục với ID đã cung cấp."
                    };
                }

                var courseContentItem = new Course_Content_Item
                {
                    ContentId = createDto.ContentId,
                    ItemTypeId = createDto.ItemTypeId,
                    ItemDes = createDto.ItemDes,
                    Status = createDto.Status
                };

                await _unitOfWork.CourseContentItemRepository.AddAsync(courseContentItem);

                await _unitOfWork.SaveChangeAsync();

                var responseDto = _mapper.Map<CourseContentItemDTO>(courseContentItem);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã thêm nội dung khóa học thành công.",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi thêm nội dung khóa học: {ex.Message}"
                };
            }
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
        public async Task<ResponseDTO> UpdateContentItemsStatusForPurchaseAsync(int coursePackageId, int learnerId)
        {
            try
            {
                var courseContents = await _unitOfWork.CourseContentRepository.GetWithIncludesAsync(
                    cc => cc.CoursePackageId == coursePackageId,
                    "CourseContentItems");

                if (courseContents == null || !courseContents.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy nội dung khóa học cho gói học này."
                    };
                }

                int updatedItemsCount = 0;

                foreach (var content in courseContents)
                {
                    if (content.CourseContentItems != null && content.CourseContentItems.Any())
                    {
                        foreach (var item in content.CourseContentItems)
                        {
                            if (item.Status == Model.Enum.CourseContentItemStatus.Free)
                            {
                                item.Status = Model.Enum.CourseContentItemStatus.Paid;
                                await _unitOfWork.CourseContentItemRepository.UpdateAsync(item);
                                updatedItemsCount++;
                            }
                        }
                    }
                }

                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Đã cập nhật {updatedItemsCount} mục nội dung từ mua sang đã miễn phí.",
                    Data = updatedItemsCount
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật trạng thái nội dung khóa học: {ex.Message}"
                };
            }
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
