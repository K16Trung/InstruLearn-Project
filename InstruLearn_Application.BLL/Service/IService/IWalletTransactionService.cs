using InstruLearn_Application.Model.Models.DTO.WalletTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IWalletTransactionService
    {
        Task<List<WalletTransactionDTO>> GetAllTransactionsAsync();
        Task<List<WalletTransactionDTO>> GetTransactionsByWalletIdAsync(int walletId);
    }
}
