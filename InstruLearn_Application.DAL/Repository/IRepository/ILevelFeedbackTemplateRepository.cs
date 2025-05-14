using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ILevelFeedbackTemplateRepository : IGenericRepository<LevelFeedbackTemplate>
    {
        Task<LevelFeedbackTemplate> GetTemplateWithCriteriaAsync(int templateId);
        Task<IEnumerable<LevelFeedbackTemplate>> GetAllTemplatesWithCriteriaAsync();
        Task<LevelFeedbackTemplate> GetTemplateForLevelAsync(int levelId);
        Task<LevelFeedbackTemplate> GetAsync(Expression<Func<LevelFeedbackTemplate, bool>> filter);
        Task<IEnumerable<LevelFeedbackTemplate>> GetAllAsync(Expression<Func<LevelFeedbackTemplate, bool>> filter = null);

    }
}
