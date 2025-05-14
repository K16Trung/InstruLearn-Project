using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IClassFeedbackRepository : IGenericRepository<ClassFeedback>
    {
        Task<ClassFeedback> GetFeedbackWithEvaluationsAsync(int feedbackId);
        Task<IEnumerable<ClassFeedback>> GetFeedbacksByClassIdAsync(int classId);
        Task<IEnumerable<ClassFeedback>> GetFeedbacksByLearnerIdAsync(int learnerId);
        Task<ClassFeedback> GetFeedbackByClassAndLearnerAsync(int classId, int learnerId);
        Task<ClassFeedback> GetAsync(Expression<Func<ClassFeedback, bool>> filter);
        Task<IEnumerable<ClassFeedback>> GetAllAsync(Expression<Func<ClassFeedback, bool>> filter = null);
    }
}
