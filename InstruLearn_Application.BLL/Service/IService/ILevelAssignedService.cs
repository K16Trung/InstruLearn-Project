using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LevelAssigned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ILevelAssignedService
    {
        Task<List<ResponseDTO>> GetAllLevelAssigned();
        Task<ResponseDTO> GetLevelAssignedById(int levelAssignedId);
        Task<ResponseDTO> CreateLevelAssigned(CreateLevelAssignedDTO createLevelAssignedDTO);
        Task<ResponseDTO> UpdateLevelAssigned(int levelAssignedId, UpdateLevelAssignedDTO createLevelAssignedDTO);
        Task<ResponseDTO> DeleteLevelAssigned(int levelAssignedId);
    }
}
