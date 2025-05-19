using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.SelfAssessment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ISelfAssessmentService
    {
        Task<ResponseDTO> GetAllAsync();
        Task<ResponseDTO> GetByIdAsync(int id);
        Task<ResponseDTO> CreateAsync(CreateSelfAssessmentDTO createDTO);
        Task<ResponseDTO> UpdateAsync(int id, UpdateSelfAssessmentDTO updateDTO);
        Task<ResponseDTO> DeleteAsync(int id);
        Task<ResponseDTO> GetByIdWithRegistrationsAsync(int id);
    }
}
