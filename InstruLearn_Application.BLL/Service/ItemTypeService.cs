using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO.ItemTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ItemTypeService : IItemTypeService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ItemTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> GetAllItemTypeAsync()
        {
            var itemType = await _unitOfWork.ItemTypeRepository.GetAllAsync();
            var itemTypeDtos = _mapper.Map<IEnumerable<ItemTypeDTO>>(itemType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Item type retrieved successfully.",
                Data = itemTypeDtos
            };
        }

        public async Task<ResponseDTO> GetItemTypeByIdAsync(int itemTypeId)
        {
            var itemType = await _unitOfWork.ItemTypeRepository.GetByIdAsync(itemTypeId);
            if (itemType == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Item type not found."
                };
            }
            var itemTypeDto = _mapper.Map<ItemTypeDTO>(itemType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Item type retrieved successfully.",
                Data = itemTypeDto
            };
        }

        public async Task<ResponseDTO> AddItemTypeAsync(CreateItemTypeDTO createDto)
        {
            var itemType = _mapper.Map<ItemTypes>(createDto);
            await _unitOfWork.ItemTypeRepository.AddAsync(itemType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Item Type added successfully.",
            };
        }
        public async Task<ResponseDTO> UpdateItemTypeAsync(int itemTypeId, UpdateItemTypeDTO updateDto)
        {
            var existingItemType = await _unitOfWork.ItemTypeRepository.GetByIdAsync(itemTypeId);
            if (existingItemType == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Item type not found."
                };
            }
            _mapper.Map(updateDto, existingItemType);
            await _unitOfWork.ItemTypeRepository.UpdateAsync(existingItemType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Item type updated successfully."
            };
        }

        public async Task<ResponseDTO> DeleteItemTypeAsync(int itemTypeId)
        {
            var itemType = await _unitOfWork.ItemTypeRepository.GetByIdAsync(itemTypeId);
            if (itemType == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Item type not found."
                };
            }
            await _unitOfWork.ItemTypeRepository.DeleteAsync(itemTypeId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Item type deleted successfully."
            };
        }
    }
}
