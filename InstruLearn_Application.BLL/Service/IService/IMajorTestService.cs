using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.MajorTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IMajorTestService
    {
        Task<List<ResponseDTO>> GetAllMajorTestsAsync();
        Task<ResponseDTO> GetMajorTestByIdAsync(int majorTestId);
        Task<ResponseDTO> CreateMajorTestAsync(CreateMajorTestDTO createMajorTestDTO);
        Task<ResponseDTO> UpdateMajorTestAsync(int majorTestId, UpdateMajorTestDTO updateMajorTestDTO);
        Task<ResponseDTO> DeleteMajorTestAsync(int majorTestId);
        Task<ResponseDTO> GetMajorTestsByMajorIdAsync(int majorId);

    }
}
