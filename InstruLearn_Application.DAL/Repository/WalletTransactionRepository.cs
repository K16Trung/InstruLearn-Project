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
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public WalletTransactionRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<WalletTransaction?> GetTransactionWithWalletAsync(string transactionId)
        {
            return await _appDbContext.Set<WalletTransaction>()
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }
        public async Task<WalletTransaction?> GetOrderCodeWithWalletAsync(long orderCode)
        {
            return await _appDbContext.Set<WalletTransaction>()
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.OrderCode == orderCode);
        }
        public async Task<List<WalletTransaction>> GetAllTransactionsAsync()
        {
            return await _appDbContext.WalletTransactions
                .Include(wt => wt.Wallet)
                    .ThenInclude(w => w.Learner)
                .OrderByDescending(wt => wt.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<WalletTransaction>> GetTransactionsByWalletIdAsync(int walletId)
        {
            return await _appDbContext.WalletTransactions
                .Where(wt => wt.WalletId == walletId)
                .Include(wt => wt.Wallet)
                    .ThenInclude(w => w.Learner)
                .OrderByDescending(wt => wt.TransactionDate)
                .ToListAsync();
        }
    }
}
