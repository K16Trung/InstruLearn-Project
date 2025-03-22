using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class TestResultRepository : GenericRepository<Test_Result>, ITestResultRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public TestResultRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Test_Result?> GetByLearningRegisIdAsync(int learningRegisId)
        {
            return await _appDbContext.Set<Test_Result>().FirstOrDefaultAsync(tr => tr.LearningRegisId == learningRegisId);
        }

        public async Task<List<Test_Result>> GetTestResultsByLearningRegisIdAsync(int learningRegisId)
        {
            return await _appDbContext.Test_Results
                .Include(tr => tr.LearningRegistration)       
                .Include(tr => tr.Learner)                    
                .Include(tr => tr.Teacher)                    
                .Where(tr => tr.LearningRegisId == learningRegisId)
                .ToListAsync();
        }

        public async Task<List<Test_Result>> GetTestResultsByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.Test_Results
                .Include(tr => tr.LearningRegistration)  
                .Include(tr => tr.Teacher)               
                .Include(tr => tr.Major)                 
                .Where(tr => tr.LearnerId == learnerId)
                .ToListAsync();
        }

        public async Task<List<Test_Result>> GetAllWithDetailsAsync()
        {
            return await _appDbContext.Test_Results
                .Include(tr => tr.LearningRegistration)  
                .Include(tr => tr.Teacher)              
                .Include(tr => tr.Major)
                .ToListAsync();
        }

        public async Task<Test_Result> GetByIdWithDetailsAsync(int testResultId)
        {
            return await _appDbContext.Test_Results
                .Include(tr => tr.LearningRegistration)  
                .Include(tr => tr.Teacher)               
                .Include(tr => tr.Major)
                .FirstOrDefaultAsync(tr => tr.TestResultId == testResultId);
        }



    }
}
