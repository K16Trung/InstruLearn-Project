using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ITeacherEvaluationService
    {
        Task<ResponseDTO> GetEvaluationByIdAsync(int evaluationFeedbackId);
        Task<ResponseDTO> GetEvaluationByRegistrationIdAsync(int learningRegistrationId);
        Task<ResponseDTO> GetEvaluationsByTeacherIdAsync(int teacherId);
        Task<ResponseDTO> GetEvaluationsByLearnerIdAsync(int learnerId);
        Task<ResponseDTO> GetPendingEvaluationsForTeacherAsync(int teacherId);
        Task<ResponseDTO> CreateEvaluationAsync(int learningRegistrationId);
        Task<ResponseDTO> SubmitEvaluationAsync(SubmitTeacherEvaluationDTO submitDTO);
        Task<ResponseDTO> CheckAndCreateEvaluationRequestsAsync();
        Task<ResponseDTO> GetActiveQuestionsAsync();
    }
}
