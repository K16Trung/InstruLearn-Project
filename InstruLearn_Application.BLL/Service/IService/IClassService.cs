using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IClassService
    {
        Task<ResponseDTO> GetAllClassAsync();
        Task<ResponseDTO> GetClassByIdAsync (string id);
        Task<ResponseDTO> AddClassAsync (CreateClassDTO createClassDTO);
        Task<ResponseDTO> UpdateClassAsync (UpdateClassDTO updateClassDTO);
        Task<ResponseDTO> DeleteClassAsync (string id);
    }
}
