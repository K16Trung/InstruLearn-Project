using InstruLearn_Application.Model.Models.DTO.Course_Content_Item;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICourseContentItemService
    {
        Task<ResponseDTO> GetAllCourseContentItemsAsync();
        Task<ResponseDTO> GetCourseContentItemByIdAsync(int itemId);
        Task<ResponseDTO> AddCourseContentItemAsync(CreateCourseContentItemDTO createDto);
        Task<ResponseDTO> UpdateCourseContentItemAsync(int itemId, UpdateCourseContentItemDTO updateDto);
        Task<ResponseDTO> DeleteCourseContentItemAsync(int itemId);
    }
}
