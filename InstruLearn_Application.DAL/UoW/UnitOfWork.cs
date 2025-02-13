using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.UoW
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILearnerRepository _learnerRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ApplicationDbContext _dbContext;
        private bool disposed = false;

        public IAccountRepository AccountRepository { get { return _accountRepository; } }
        public ILearnerRepository LearnerRepository { get { return _learnerRepository; } }
        public ITeacherRepository TeacherRepository { get { return _teacherRepository; } }
        public ApplicationDbContext dbContext { get { return _dbContext; } }

        public UnitOfWork(ApplicationDbContext dbContext, IAccountRepository accountRepository, ILearnerRepository learnerRepository, ITeacherRepository teacherRepository)
        {
            _dbContext = dbContext;
            _accountRepository = accountRepository;
            _learnerRepository = learnerRepository;
            _teacherRepository = teacherRepository;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
