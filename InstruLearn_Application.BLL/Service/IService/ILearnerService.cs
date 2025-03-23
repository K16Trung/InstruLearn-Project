using InstruLearn_Application.Model.Models.DTO.Learner;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ILearnerService
    {
        Task<ResponseDTO> GetAllLearnerAsync();
        Task<ResponseDTO> GetLearnerByIdAsync(int learnerId);
        Task<ResponseDTO> UpdateLearnerAsync(int learnerId, UpdateLearnerDTO updateLearnerDTO);
        Task<ResponseDTO> DeleteLearnerAsync(int learnerId);
        Task<ResponseDTO> UnbanLearnerAsync(int learnerId);
    }
}
