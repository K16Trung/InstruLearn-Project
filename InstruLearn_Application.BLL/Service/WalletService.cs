using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Configuration;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class WalletService : IWalletService
    {
        private readonly PayOSSettings _payOSSettings;
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(PayOSSettings payOSSettings, IUnitOfWork unitOfWork)
        {
            _payOSSettings = payOSSettings;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> AddFundsToWallet(int learnerId, decimal amount)
        {
            var wallet = await _unitOfWork.WalletRepository.FirstOrDefaultAsync(w => w.LearnerId == learnerId);
            if (wallet == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Wallet not found" };
            }

            // Create a wallet transaction
            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                TransactionId = Guid.NewGuid().ToString(),
                TransactionType = TransactionType.AddFuns,
                Status = TransactionStatus.Pending,
                TransactionDate = DateTime.Now
            };

            await _unitOfWork.WalletTransactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangeAsync();

            // Use PayOS to generate a payment link
            PayOS payOS = new PayOS(_payOSSettings.ClientId, _payOSSettings.ApiKey, _payOSSettings.ChecksumKey);
            List<ItemData> items = new List<ItemData>
            {
                new ItemData("Add Funds to Wallet", 1, (int)amount)
            };

                PaymentData paymentData = new PaymentData(
                orderCode: new Random().Next(100000, 999999),
                amount: (int)amount,
                description: "Add Funds to Wallet",
                items: items,
                cancelUrl: "https://www.facebook.com/FPTU.HCM",
                returnUrl: "https://fap.fpt.edu.vn/"
                );

            var createPayment = await payOS.createPaymentLink(paymentData);

            if (createPayment == null || string.IsNullOrEmpty(createPayment.checkoutUrl))
            {
                return new ResponseDTO { IsSucceed = false, Message = "Failed to generate payment link" };
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Payment link created",
                Data = new
                {
                    TransactionId = transaction.TransactionId,
                    Amount = transaction.Amount,
                    Status = transaction.Status.ToString(),
                    PaymentUrl = createPayment.checkoutUrl
                }
            };
        }

        public async Task<ResponseDTO> UpdatePaymentStatusAsync(string orderCode, string status)
        {
            var transaction = await _unitOfWork.WalletTransactionRepository
                .FirstOrDefaultAsync(t => t.TransactionId == orderCode);

            if (transaction == null)
            {
                return new ResponseDTO { IsSucceed = false, Message = "Transaction not found" };
            }

            if (status.ToUpper() == "PAID")
            {
                transaction.Status = TransactionStatus.Complete;
                transaction.Wallet.Balance += transaction.Amount;
            }
            else if (status.ToUpper() == "FAILED" || status.ToUpper() == "CANCELED")
            {
                transaction.Status = TransactionStatus.Failed;
            }

            await _unitOfWork.SaveChangeAsync();
            return new ResponseDTO { IsSucceed = true, Message = "Payment status updated" };
        }
    }
}
