using InstruLearn_Application.Model.Models.DTO.Staff;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IStaffService
    {
        Task<ResponseDTO> CreateStaffAsync(CreateStaffDTO createStaffDTO);
        Task<ResponseDTO> GetStaffByIdAsync(int staffId);
        Task<ResponseDTO> UpdateStaffAsync(int staffId, UpdateStaffDTO updateStaffDTO);
        Task<ResponseDTO> DeleteStaffAsync(int staffId);
        Task<ResponseDTO> UnbanStaffAsync(int staffId);
        Task<ResponseDTO> GetAllStaffAsync();
    }
}
