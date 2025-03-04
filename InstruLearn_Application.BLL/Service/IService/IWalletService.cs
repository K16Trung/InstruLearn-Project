using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IWalletService
    {
        Task<ResponseDTO> AddFundsToWallet(int learnerId, decimal amount);
        Task<ResponseDTO> UpdatePaymentStatusAsync(string orderCode, string status);
        Task<ResponseDTO> GetWalletByLearnerIdAsync(int learnerId);
    }
}
