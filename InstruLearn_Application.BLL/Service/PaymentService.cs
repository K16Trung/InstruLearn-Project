using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Payment;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                decimal totalPrice = learningRegis.Price.Value;
                decimal requiredAmount = totalPrice * 0.4m;
                decimal remainingAmount = totalPrice * 0.6m;

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
                // FIXED: Use PaymentId for the LearningRegis ID since we don't have a separate property
                var payment = new Payment
                {
                    WalletId = wallet.WalletId,
                    TransactionId = walletTransaction.TransactionId,
                    AmountPaid = requiredAmount,
                    PaymentMethod = paymentDTO.PaymentMethod,
                    PaymentFor = PaymentFor.LearningRegistration,
                    Status = PaymentStatus.Completed
                    // LearningRegisId property was removed as it doesn't exist
                };
                await _unitOfWork.PaymentsRepository.AddAsync(payment);

                // Update Learning Registration Status
                learningRegis.Status = LearningRegis.Fourty;
                learningRegis.RemainingAmount = remainingAmount;
                learningRegis.HasPendingLearningPath = true;
                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                // Find and resolve any existing CreateLearningPath notifications
                var existingNotifications = await _unitOfWork.StaffNotificationRepository
                    .GetQuery()
                    .Where(n => n.LearningRegisId == learningRegis.LearningRegisId &&
                                n.Type == NotificationType.CreateLearningPath &&
                                n.Status != NotificationStatus.Resolved)
                    .ToListAsync();

                foreach (var notification in existingNotifications)
                {
                    notification.Status = NotificationStatus.Resolved;
                    await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                }

                // ✅ Create schedules for learner and teacher
                var schedules = DateTimeHelper.GenerateOneOnOneSchedules(learningRegis);
                await _unitOfWork.ScheduleRepository.AddRangeAsync(schedules);

                // Create notification for teacher about schedules being updated
                if (learningRegis.TeacherId.HasValue)
                {
                    var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(learningRegis.TeacherId.Value);
                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learningRegis.LearnerId);

                    if (teacher != null && learner != null)
                    {
                        var staffNotification = new StaffNotification
                        {
                            Title = "Lịch trình đã tạo - Thanh toán đã nhận",
                            Message = $"Học viên {learner.FullName} đã thanh toán 40% cho đơn đăng kí: {learningRegis.LearningRegisId}. " +
                                      $"Lịch trình giảng dạy của bạn đã được tạo và hiện có trong lịch của bạn.",
                            LearningRegisId = learningRegis.LearningRegisId,
                            LearnerId = learningRegis.LearnerId,
                            CreatedAt = DateTime.Now,
                            Status = NotificationStatus.Unread,
                            Type = NotificationType.SchedulesCreated
                        };

                        await _unitOfWork.StaffNotificationRepository.AddAsync(staffNotification);
                    }
                }

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


        public async Task<ResponseDTO> ProcessRemainingPaymentAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Learning Registration not found." };
                }

                if (learningRegis.RemainingAmount == null || learningRegis.RemainingAmount <= 0)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "No remaining payment required." };
                }

                var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == learningRegis.LearnerId);
                if (wallet == null)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Wallet not found." };
                }

                if (wallet.Balance < learningRegis.RemainingAmount)
                {
                    return new ResponseDTO { IsSucceed = false, Message = "Insufficient balance in wallet." };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                // Deduct from Wallet Balance
                wallet.Balance -= learningRegis.RemainingAmount.Value;
                wallet.UpdateAt = DateTime.UtcNow;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);

                // Create Wallet Transaction
                var walletTransaction = new WalletTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    WalletId = wallet.WalletId,
                    Amount = learningRegis.RemainingAmount.Value,
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
                    AmountPaid = learningRegis.RemainingAmount.Value,
                    PaymentMethod = PaymentMethod.Wallet,
                    PaymentFor = PaymentFor.LearningRegistration,
                    Status = PaymentStatus.Completed
                    // LearningRegisId property was removed as it doesn't exist
                };
                await _unitOfWork.PaymentsRepository.AddAsync(payment);

                // Update Learning Registration Status and Clear Remaining Amount
                learningRegis.Status = LearningRegis.Sixty;
                learningRegis.RemainingAmount = 0;
                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                // Find and mark any related PaymentReminder notifications as resolved
                var notifications = await _unitOfWork.StaffNotificationRepository
                    .GetQuery()
                    .Where(n => n.LearningRegisId == learningRegisId &&
                               n.Type == NotificationType.PaymentReminder &&
                               n.Status != NotificationStatus.Resolved)
                    .ToListAsync();

                if (notifications.Any())
                {
                    foreach (var notification in notifications)
                    {
                        notification.Status = NotificationStatus.Resolved;
                        await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                    }
                }

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Remaining payment successful.",
                    Data = null
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