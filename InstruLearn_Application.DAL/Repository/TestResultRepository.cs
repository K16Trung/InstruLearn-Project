using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
