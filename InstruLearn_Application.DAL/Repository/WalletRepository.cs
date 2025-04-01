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

        public async Task<Wallet> GetFirstOrDefaultAsync(Expression<Func<Wallet, bool>> predicate)
        {
            return await _appDbContext.Wallets.FirstOrDefaultAsync(predicate);
        }

        /*public async Task<WalletTransaction> GetTransactionWithWalletAsync(string orderCode)
        {
            long orderCodeValue;
            if (!long.TryParse(orderCode, out orderCodeValue))
            {
                return null;
            }

            return await _appDbContext.WalletTransactions
                .Include(wt => wt.Wallet)
                .FirstOrDefaultAsync(wt => wt.OrderCode == orderCodeValue);
        }*/

    }
}
