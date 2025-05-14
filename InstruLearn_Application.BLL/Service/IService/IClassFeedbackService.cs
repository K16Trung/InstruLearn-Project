using InstruLearn_Application.Model.Models.DTO.ClassFeedback;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IClassFeedbackService
    {
        Task<ResponseDTO> CreateFeedbackAsync(CreateClassFeedbackDTO feedbackDTO);
        Task<ResponseDTO> UpdateFeedbackAsync(int feedbackId, UpdateClassFeedbackDTO feedbackDTO);
        Task<ResponseDTO> DeleteFeedbackAsync(int feedbackId);
        Task<ClassFeedbackDTO> GetFeedbackAsync(int feedbackId);
        Task<List<ClassFeedbackDTO>> GetFeedbacksByClassIdAsync(int classId);
        Task<List<ClassFeedbackDTO>> GetFeedbacksByLearnerIdAsync(int learnerId);
        Task<ClassFeedbackDTO> GetFeedbackByClassAndLearnerAsync(int classId, int learnerId);
        Task<ClassFeedbackSummaryDTO> GetFeedbackSummaryForClassAsync(int classId);
    }
}
