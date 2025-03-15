using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningRegistrationType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ILearningRegisTypeService
    {
        Task<ResponseDTO> GetAllLearningRegisTypeAsync();
        Task<ResponseDTO> GetLearningRegisTypeByIdAsync(int learningRegisTypeId);
        Task<ResponseDTO> CreateLearningRegisTypeAsync(CreateTypeDTO createTypeDTO);
        Task<ResponseDTO> DeleteLearningRegisTypeAsync(int learningRegisTypeId);
    }
}
