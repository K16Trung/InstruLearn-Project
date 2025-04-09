using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Vnpay;
using InstruLearn_Application.Model.Models.DTO.Wallet;
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
        Task<ResponseDTO> AddFundsWithVnpay(int learnerId, decimal amount, string ipAddress);
        Task<ResponseDTO> UpdatePaymentStatusAsync(string transactionId);
        Task<ResponseDTO> ProcessVnpayReturnAsync(VnpayPaymentResponse paymentResponse);
        Task<ResponseDTO> FailPaymentAsync(string transactionId);
        Task<ResponseDTO> UpdatePaymentStatusByOrderCodeAsync(long orderCode, string status);
        Task<ResponseDTO> GetWalletByLearnerIdAsync(int learnerId);
    }
}