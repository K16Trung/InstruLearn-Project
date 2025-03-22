using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface ITestResultRepository : IGenericRepository<Test_Result>
    {
        Task<Test_Result?> GetByLearningRegisIdAsync(int learningRegisId);

    }
}
