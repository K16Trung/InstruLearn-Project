using InstruLearn_Application.Model.Models.DTO.Manager;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IManagerService
    {
        Task<ResponseDTO> CreateManagerAsync(CreateManagerDTO createManagerDTO);
        Task<ResponseDTO> GetManagerByIdAsync(int managerId);
        Task<ResponseDTO> UpdateManagerAsync(int managerId, UpdateManagerDTO updateManagerDTO);
        Task<ResponseDTO> DeleteManagerAsync(int managerId);
        Task<ResponseDTO> UnbanManagerAsync(int managerId);
        Task<ResponseDTO> GetAllManagerAsync();
    }
}
