using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.Major;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IMajorService
    {
        Task<ResponseDTO> GetAllMajorAsync();
        Task<ResponseDTO> GetMajorByIdAsync(int majorId);
        Task<ResponseDTO> AddMajorAsync(CreateMajorDTO createDto);
        Task<ResponseDTO> UpdateMajorAsync(int majorId, UpdateMajorDTO updateDto);
        Task<ResponseDTO> UpdateStatusMajorAsync(int majorId);
        Task<ResponseDTO> DeleteMajorAsync(int majorId);
    }
}
