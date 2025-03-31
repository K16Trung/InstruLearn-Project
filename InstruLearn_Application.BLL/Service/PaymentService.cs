using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.Payment;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using InstruLearn_Application.DAL.UoW.IUoW;

namespace InstruLearn_Application.BLL.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> ProcessLearningRegisPaymentAsync(CreatePaymentDTO paymentDTO)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(paymentDTO.LearningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Learning Registration not found." };
                }

                if (learningRegis.Price == null || learningRegis.Price <= 0)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Invalid learning registration price." };
                }

                var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == learningRegis.LearnerId);
                if (wallet == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Wallet not found." };
                }

                decimal requiredAmount = learningRegis.Price.Value * 0.4m; // 40% of total price

                if (wallet.Balance < requiredAmount)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Insufficient balance in wallet." };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                // Deduct from Wallet Balance
                wallet.Balance -= requiredAmount;
                wallet.UpdateAt = DateTime.UtcNow;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);

                // Create Wallet Transaction
                var walletTransaction = new WalletTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    WalletId = wallet.WalletId,
                    Amount = requiredAmount,
                    TransactionType = TransactionType.Payment,
                    Status = TransactionStatus.Complete,
                    TransactionDate = DateTime.UtcNow
                };
                await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);

                // Create Payment Record
                var payment = new Payment
                {
                    WalletId = wallet.WalletId,
                    TransactionId = walletTransaction.TransactionId,
                    AmountPaid = requiredAmount,
                    PaymentMethod = paymentDTO.PaymentMethod,
                    PaymentFor = PaymentFor.LearningRegistration,
                    Status = PaymentStatus.Completed
                };
                await _unitOfWork.PaymentsRepository.AddAsync(payment);

                // Update Learning Registration Status
                learningRegis.Status = LearningRegis.Completed;
                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                // ✅ Create schedules for learner and teacher
                var schedules = DateTimeHelper.GenerateOnonOnSchedules(learningRegis);
                await _unitOfWork.ScheduleRepository.AddRangeAsync(schedules);

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Map Payment to PaymentDTO using AutoMapper
                var paymentResponse = _mapper.Map<PaymentDTO>(payment);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "40% payment successful for Learning Registration.",
                    Data = paymentResponse
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO { IsSucceed = false, Message = "Payment failed. " + ex.Message };
            }
        }

    }
}
