using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearnerClass;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ILearningRegisService
    {
        Task<ResponseDTO> GetAllLearningRegisAsync();
        Task<ResponseDTO> GetLearningRegisByIdAsync(int learningRegisId);
        Task<ResponseDTO> CreateLearningRegisAsync(CreateLearningRegisDTO createLearningRegisDTO);
        Task<ResponseDTO> DeleteLearningRegisAsync(int learningRegisId);
        Task<ResponseDTO> GetAllPendingRegistrationsAsync();
        Task<ResponseDTO> GetRegistrationsByLearnerIdAsync(int learnerId);
        Task<ResponseDTO> UpdateLearningRegisStatusAsync(UpdateLearningRegisDTO updateDTO);
        Task<ResponseDTO> JoinClassWithWalletPaymentAsync(LearnerClassPaymentDTO paymentDTO);
        Task<ResponseDTO> RejectLearningRegisAsync(int learningRegisId, int? responseId);
        Task<ResponseDTO> CreateLearningPathSessionsAsync(LearningPathSessionsCreateDTO createDTO);
        Task<ResponseDTO> GetRegistrationsByTeacherIdAsync(int teacherId);
    }
}
