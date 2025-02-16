using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.ItemTypes;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IItemTypeService
    {
        Task<ResponseDTO> GetAllItemTypeAsync();
        Task<ResponseDTO> GetItemTypeByIdAsync(int itemTypeId);
        Task<ResponseDTO> AddItemTypeAsync(CreateItemTypeDTO createDto);
        Task<ResponseDTO> UpdateItemTypeAsync(int itemTypeId, UpdateItemTypeDTO updateDto);
        Task<ResponseDTO> DeleteItemTypeAsync(int itemTypeId);
    }
}
