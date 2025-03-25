using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.PayOSWebhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class PayOSWebhookService : IPayOSWebhookService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PayOSWebhookService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ProcessWebhookAsync(PayOSWebhookDTO webhookDto)
        {
            if (webhookDto == null)
            {
                throw new ArgumentNullException(nameof(webhookDto));
            }

            // Find the payment associated with this transaction
            var payment = await _unitOfWork.PaymentsRepository.GetByTransactionIdAsync(webhookDto.TransactionId);
            if (payment == null)
            {
                throw new Exception("Không tìm thấy thanh toán.");
            }

            // Update payment status
            payment.Status = webhookDto.Status == "THÀNH CÔNG" ? PaymentStatus.Completed : PaymentStatus.Failed;
            await _unitOfWork.PaymentsRepository.UpdatePaymentAsync(payment);

            // If payment is successful, add funds to wallet
            if (webhookDto.Status == "THÀNH CÔNG")
            {
                var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(payment.WalletId);
                if (wallet != null)
                {
                    wallet.Balance += payment.AmountPaid;
                    await _unitOfWork.WalletRepository.UpdateAsync(wallet);
                }
            }
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
