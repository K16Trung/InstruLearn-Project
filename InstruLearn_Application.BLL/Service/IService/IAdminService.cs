using InstruLearn_Application.Model.Models.DTO.Admin;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IAdminService
    {
        Task<ResponseDTO> CreateAdminAsync(CreateAdminDTO createAdminDTO);
        Task<ResponseDTO> GetAdminByIdAsync(int adminId);
        Task<ResponseDTO> UpdateAdminAsync(int adminId, UpdateAdminDTO updateAdminDTO);
        Task<ResponseDTO> GetAllAdminAsync();
    }
}
