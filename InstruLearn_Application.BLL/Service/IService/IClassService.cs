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
        Task<List<ClassDTO>> GetAllClassAsync();
        Task<ClassDTO> GetClassByIdAsync (int classid);
        Task<ResponseDTO> GetClassesByMajorIdAsync(int majorId);
        Task<ResponseDTO> AddClassAsync (CreateClassDTO createClassDTO);
        Task<ResponseDTO> UpdateClassAsync (int classId, UpdateClassDTO updateClassDTO);
        Task<ResponseDTO> DeleteClassAsync (int classId);
    }
}
