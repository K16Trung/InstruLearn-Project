using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearningRegisFeedbackOptionRepository : IGenericRepository<LearningRegisFeedbackOption>
    {
        Task<List<LearningRegisFeedbackOption>> GetOptionsByQuestionIdAsync(int questionId);
    }
}
