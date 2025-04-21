using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearningRegisFeedbackRepository : IGenericRepository<LearningRegisFeedback>
    {
        Task<LearningRegisFeedback> GetFeedbackWithDetailsAsync(int feedbackId);
        Task<LearningRegisFeedback> GetFeedbackByRegistrationIdAsync(int learningRegistrationId);
        Task<List<LearningRegisFeedback>> GetFeedbacksByTeacherIdAsync(int teacherId);
        Task<List<LearningRegisFeedback>> GetFeedbacksByLearnerIdAsync(int learnerId);
    }
}
