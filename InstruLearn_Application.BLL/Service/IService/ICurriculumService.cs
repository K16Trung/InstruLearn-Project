using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Curriculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ICurriculumService
    {
        Task<List<CurriculumDTO>> GetAllCurriculumAsync();
        Task<CurriculumDTO> GetCurriculumByIdAsync(int curriculumId);
        Task<ResponseDTO> AddCurriculumAsync(CreateCurriculumDTO createCurriculumDTO);
        Task<ResponseDTO> UpdateCurriculumAsync(int curriculumId, UpdateCurriculumDTO updateCurriculumDTO);
        Task<ResponseDTO> DeleteCurriculumAsync(int curriculumId);
    }
}
