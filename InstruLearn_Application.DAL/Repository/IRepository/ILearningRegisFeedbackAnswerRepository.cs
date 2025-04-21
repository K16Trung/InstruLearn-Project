using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILearningRegisFeedbackAnswerRepository : IGenericRepository<LearningRegisFeedbackAnswer>
    {
        Task<List<LearningRegisFeedbackAnswer>> GetAnswersByFeedbackIdAsync(int feedbackId);
    }
}
