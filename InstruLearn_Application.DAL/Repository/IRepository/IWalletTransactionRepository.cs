using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IWalletTransactionRepository : IGenericRepository<WalletTransaction>
    {
        Task<WalletTransaction?> GetTransactionWithWalletAsync(string transactionId);
        Task<List<WalletTransaction>> GetTransactionsByWalletIdAsync(int walletId);
        Task<List<WalletTransaction>> GetAllTransactionsAsync();
    }
}
