using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public WalletRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<Wallet> GetWalletByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.Wallets
                .Include(w => w.Learner)
                    .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(w => w.LearnerId == learnerId);
        }
    }
}
