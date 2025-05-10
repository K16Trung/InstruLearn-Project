using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class PaymentInfoService : IPaymentInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentInfoService> _logger;

        public PaymentInfoService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentInfoService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDTO> GetPaymentPeriodsInfoAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy đăng ký học."
                    };
                }

                // Calculate payment amounts
                decimal totalPrice = learningRegis.Price ?? 0;
                decimal firstPaymentAmount = Math.Round(totalPrice * 0.4m, 0);
                decimal secondPaymentAmount = Math.Round(totalPrice * 0.6m, 0);

                // Initialize payment status variables
                var firstPaymentCompleted = false;
                var secondPaymentCompleted = false;
                string firstPaymentStatus = "Chưa thanh toán";
                string secondPaymentStatus = "Chưa thanh toán";
                DateTime? firstPaymentDate = null;
                DateTime? secondPaymentDate = null;

                // Get payment transactions
                var payments = await _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => p.PaymentFor == PaymentFor.LearningRegistration &&
                                p.PaymentId == learningRegisId &&
                                p.Status == PaymentStatus.Completed)
                    .ToListAsync();

                _logger.LogInformation($"Found {payments.Count} completed payments for learning registration {learningRegisId}");

                // CRITICAL FIX: Check learning registration status FIRST
                // This is more reliable across environments
                if (learningRegis.Status == LearningRegis.Fourty ||
                    learningRegis.Status == LearningRegis.FourtyFeedbackDone ||
                    learningRegis.Status == LearningRegis.Sixty)
                {
                    _logger.LogInformation($"Learning reg status is {learningRegis.Status}. Setting first payment as completed.");
                    firstPaymentCompleted = true;
                    firstPaymentStatus = "Đã thanh toán";  // Always set to "Đã thanh toán" when status is Fourty or beyond
                }

                if (learningRegis.Status == LearningRegis.Sixty)
                {
                    _logger.LogInformation($"Learning reg status is {learningRegis.Status}. Setting second payment as completed.");
                    secondPaymentCompleted = true;
                    secondPaymentStatus = "Đã thanh toán";  // Always set to "Đã thanh toán" when status is Sixty
                }

                // After setting status based on LearningRegis.Status, try to find payment dates from payments
                if (payments != null && payments.Any())
                {
                    foreach (var payment in payments)
                    {
                        _logger.LogInformation($"Processing payment: Amount={payment.AmountPaid}, TransactionId={payment.TransactionId}");

                        var transaction = await _unitOfWork.WalletTransactionRepository
                            .GetTransactionWithWalletAsync(payment.TransactionId);

                        if (transaction != null)
                        {
                            // If there's a payment close to 40% amount (with 0.1 margin for rounding)
                            if (Math.Abs(payment.AmountPaid - firstPaymentAmount) < 0.1m)
                            {
                                _logger.LogInformation($"Found 40% payment (Amount: {payment.AmountPaid}) for learning registration {learningRegisId}");
                                if (firstPaymentDate == null)
                                {
                                    firstPaymentDate = transaction.TransactionDate;
                                    // Ensure status is set to completed as there's a valid payment
                                    firstPaymentCompleted = true;
                                    firstPaymentStatus = "Đã thanh toán";
                                }
                            }
                            // If there's a payment close to 60% amount
                            else if (Math.Abs(payment.AmountPaid - secondPaymentAmount) < 0.1m)
                            {
                                _logger.LogInformation($"Found 60% payment (Amount: {payment.AmountPaid}) for learning registration {learningRegisId}");
                                if (secondPaymentDate == null)
                                {
                                    secondPaymentDate = transaction.TransactionDate;
                                    // Ensure status is set to completed as there's a valid payment
                                    secondPaymentCompleted = true;
                                    secondPaymentStatus = "Đã thanh toán";
                                }
                            }
                        }
                    }
                }

                // Initialize deadline variables
                int? firstPaymentRemainingDays = null;
                int? secondPaymentRemainingDays = null;
                DateTime? firstPaymentDeadline = null;
                DateTime? secondPaymentDeadline = null;

                // Determine which phase this registration is in
                bool isInFirstPaymentPhase = !firstPaymentCompleted;
                bool isInSecondPaymentPhase = firstPaymentCompleted && !secondPaymentCompleted;

                // Handle payment deadlines
                if (learningRegis.PaymentDeadline.HasValue)
                {
                    // If we're in first payment phase
                    if (isInFirstPaymentPhase)
                    {
                        firstPaymentDeadline = learningRegis.PaymentDeadline;

                        // Calculate remaining days
                        DateTime now = DateTime.Now.Date; // Use date to ignore time component
                        DateTime deadline = firstPaymentDeadline.Value.Date; // Use date to ignore time component

                        // Calculate days between now and deadline
                        int daysDifference = (deadline - now).Days;

                        firstPaymentRemainingDays = daysDifference;

                        // Ensure we don't show negative days
                        if (firstPaymentRemainingDays < 0)
                            firstPaymentRemainingDays = 0;

                        // Handle overdue case
                        if (daysDifference < 0 && !firstPaymentCompleted)
                        {
                            firstPaymentStatus = "Đã quá hạn thanh toán 40%";

                            if (learningRegis.Status != LearningRegis.Rejected)
                            {
                                using var transaction = await _unitOfWork.BeginTransactionAsync();
                                try
                                {
                                    learningRegis.Status = LearningRegis.Rejected;
                                    learningRegis.LearningRequest = "Quá hạn thanh toán 40%";

                                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                                    var testResult = await _unitOfWork.TestResultRepository
                                        .GetByLearningRegisIdAsync(learningRegis.LearningRegisId);

                                    if (testResult != null)
                                    {
                                        testResult.Status = TestResultStatus.Cancelled;
                                        await _unitOfWork.TestResultRepository.UpdateAsync(testResult);
                                    }

                                    await _unitOfWork.SaveChangeAsync();
                                    await transaction.CommitAsync();
                                }
                                catch (Exception ex)
                                {
                                    await transaction.RollbackAsync();
                                    _logger.LogError(ex, $"Error updating overdue payment status: {ex.Message}");
                                }
                            }
                        }
                    }
                    else if (isInSecondPaymentPhase)
                    {
                        secondPaymentDeadline = learningRegis.PaymentDeadline;

                        // Calculate remaining days for second payment
                        DateTime now = DateTime.Now.Date;
                        DateTime deadline = secondPaymentDeadline.Value.Date;

                        int daysDifference = (deadline - now).Days;

                        secondPaymentRemainingDays = daysDifference;

                        if (secondPaymentRemainingDays < 0)
                            secondPaymentRemainingDays = 0;

                        if (daysDifference < 0 && !secondPaymentCompleted)
                        {
                            secondPaymentStatus = "Đã quá hạn thanh toán 60%";

                            if (learningRegis.Status != LearningRegis.Cancelled)
                            {
                                using var transaction = await _unitOfWork.BeginTransactionAsync();
                                try
                                {
                                    learningRegis.Status = LearningRegis.Cancelled;
                                    learningRegis.LearningRequest = "Quá hạn thanh toán 60%";

                                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                                    var schedules = await _unitOfWork.ScheduleRepository
                                        .GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                                    foreach (var schedule in schedules)
                                    {
                                        await _unitOfWork.ScheduleRepository.DeleteAsync(schedule.ScheduleId);
                                    }

                                    await _unitOfWork.SaveChangeAsync();
                                    await transaction.CommitAsync();
                                }
                                catch (Exception ex)
                                {
                                    await transaction.RollbackAsync();
                                    _logger.LogError(ex, $"Error updating overdue payment status: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                else if (learningRegis.Status == LearningRegis.Pending || learningRegis.Status == LearningRegis.Accepted)
                {
                    if (learningRegis.AcceptedDate.HasValue)
                    {
                        firstPaymentDeadline = learningRegis.AcceptedDate.Value.AddDays(3);
                    }
                    else
                    {
                        firstPaymentDeadline = DateTime.Now.AddDays(3);
                    }

                    if (firstPaymentDeadline.HasValue)
                    {
                        DateTime now = DateTime.Now.Date;
                        DateTime deadline = firstPaymentDeadline.Value.Date;

                        int daysDifference = (deadline - now).Days;

                        firstPaymentRemainingDays = daysDifference;

                        if (firstPaymentRemainingDays < 0)
                            firstPaymentRemainingDays = 0;
                    }
                }

                var firstPaymentPeriod = new
                {
                    PaymentPercent = 40,
                    PaymentAmount = firstPaymentAmount,
                    PaymentStatus = firstPaymentStatus,
                    PaymentDeadline = firstPaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentDate = firstPaymentDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    RemainingDays = firstPaymentRemainingDays,
                    IsOverdue = firstPaymentDeadline.HasValue && !firstPaymentCompleted && DateTime.Now > firstPaymentDeadline.Value
                };

                var secondPaymentPeriod = new
                {
                    PaymentPercent = 60,
                    PaymentAmount = secondPaymentAmount,
                    PaymentStatus = secondPaymentStatus,
                    PaymentDeadline = secondPaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentDate = secondPaymentDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    RemainingDays = secondPaymentRemainingDays,
                    IsOverdue = secondPaymentDeadline.HasValue && !secondPaymentCompleted && DateTime.Now > secondPaymentDeadline.Value
                };

                _logger.LogInformation($"Payment info retrieved successfully for learning reg {learningRegisId}. " +
                                      $"First payment: {firstPaymentStatus}, Second payment: {secondPaymentStatus}");

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Thông tin thanh toán đã được lấy thành công.",
                    Data = new
                    {
                        learningRegisId = learningRegis.LearningRegisId,
                        currentStatus = learningRegis.Status.ToString(),
                        firstPaymentPeriod = firstPaymentPeriod,
                        secondPaymentPeriod = secondPaymentPeriod,
                        isFirstPaymentPhase = isInFirstPaymentPhase,
                        isSecondPaymentPhase = isInSecondPaymentPhase
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving payment periods info for learning reg {learningRegisId}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy thông tin các đợt thanh toán: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> EnrichLearningRegisWithPaymentInfoAsync(ResponseDTO learningRegisResponse)
        {
            if (!learningRegisResponse.IsSucceed || learningRegisResponse.Data == null)
            {
                return learningRegisResponse;
            }

            try
            {
                if (learningRegisResponse.Data is System.Collections.IEnumerable && !(learningRegisResponse.Data is string))
                {
                    var jsonData = JsonSerializer.Serialize(learningRegisResponse.Data);
                    var registrationsList = JsonSerializer.Deserialize<List<JsonElement>>(jsonData);

                    if (registrationsList == null || !registrationsList.Any())
                    {
                        return learningRegisResponse;
                    }

                    var enrichedRegistrations = new List<object>();

                    foreach (var registrationElement in registrationsList)
                    {
                        var registrationDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                            registrationElement.GetRawText());

                        if (registrationDict != null && registrationDict.TryGetValue("learningRegisId", out var learningRegisIdElement))
                        {
                            int learningRegisId = learningRegisIdElement.GetInt32();

                            var paymentInfoResponse = await GetPaymentPeriodsInfoAsync(learningRegisId);

                            if (paymentInfoResponse.IsSucceed && paymentInfoResponse.Data != null)
                            {
                                var paymentInfoJson = JsonSerializer.Serialize(paymentInfoResponse.Data);
                                var paymentInfoDict = JsonSerializer.Deserialize<Dictionary<string, object>>(paymentInfoJson);

                                var enrichedRegistration = new Dictionary<string, object>();
                                foreach (var kvp in registrationDict)
                                {
                                    enrichedRegistration[kvp.Key] = JsonSerializer.Deserialize<object>(kvp.Value.GetRawText());
                                }

                                if (paymentInfoDict != null)
                                {
                                    foreach (var kvp in paymentInfoDict)
                                    {
                                        enrichedRegistration[kvp.Key] = kvp.Value;
                                    }
                                }

                                enrichedRegistrations.Add(enrichedRegistration);
                            }
                            else
                            {
                                enrichedRegistrations.Add(JsonSerializer.Deserialize<object>(registrationElement.GetRawText()));
                            }
                        }
                        else
                        {
                            enrichedRegistrations.Add(JsonSerializer.Deserialize<object>(registrationElement.GetRawText()));
                        }
                    }

                    return new ResponseDTO
                    {
                        IsSucceed = learningRegisResponse.IsSucceed,
                        Message = learningRegisResponse.Message,
                        Data = enrichedRegistrations
                    };
                }
                else
                {
                    var jsonData = JsonSerializer.Serialize(learningRegisResponse.Data);
                    var registrationDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);

                    if (registrationDict != null && registrationDict.TryGetValue("learningRegisId", out var learningRegisIdElement))
                    {
                        int learningRegisId = learningRegisIdElement.GetInt32();

                        var paymentInfoResponse = await GetPaymentPeriodsInfoAsync(learningRegisId);

                        if (paymentInfoResponse.IsSucceed && paymentInfoResponse.Data != null)
                        {
                            var paymentInfoJson = JsonSerializer.Serialize(paymentInfoResponse.Data);
                            var paymentInfoDict = JsonSerializer.Deserialize<Dictionary<string, object>>(paymentInfoJson);

                            var enrichedRegistration = new Dictionary<string, object>();
                            foreach (var kvp in registrationDict)
                            {
                                enrichedRegistration[kvp.Key] = JsonSerializer.Deserialize<object>(kvp.Value.GetRawText());
                            }

                            if (paymentInfoDict != null)
                            {
                                foreach (var kvp in paymentInfoDict)
                                {
                                    enrichedRegistration[kvp.Key] = kvp.Value;
                                }
                            }

                            return new ResponseDTO
                            {
                                IsSucceed = learningRegisResponse.IsSucceed,
                                Message = learningRegisResponse.Message,
                                Data = enrichedRegistration
                            };
                        }
                    }
                }

                return learningRegisResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching learning registrations with payment info");
                return learningRegisResponse;
            }
        }

        public async Task<ResponseDTO> EnrichSingleLearningRegisWithPaymentInfoAsync(int learningRegisId, ResponseDTO learningRegisResponse)
        {
            if (!learningRegisResponse.IsSucceed || learningRegisResponse.Data == null)
            {
                return learningRegisResponse;
            }

            try
            {
                var paymentInfoResponse = await GetPaymentPeriodsInfoAsync(learningRegisId);

                if (paymentInfoResponse.IsSucceed && paymentInfoResponse.Data != null)
                {
                    var registrationJson = JsonSerializer.Serialize(learningRegisResponse.Data);
                    var paymentInfoJson = JsonSerializer.Serialize(paymentInfoResponse.Data);

                    var registrationDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(registrationJson);
                    var paymentInfoDict = JsonSerializer.Deserialize<Dictionary<string, object>>(paymentInfoJson);

                    if (registrationDict != null)
                    {
                        var enrichedRegistration = new Dictionary<string, object>();
                        foreach (var kvp in registrationDict)
                        {
                            enrichedRegistration[kvp.Key] = JsonSerializer.Deserialize<object>(kvp.Value.GetRawText());
                        }

                        if (paymentInfoDict != null)
                        {
                            foreach (var kvp in paymentInfoDict)
                            {
                                enrichedRegistration[kvp.Key] = kvp.Value;
                            }
                        }

                        return new ResponseDTO
                        {
                            IsSucceed = learningRegisResponse.IsSucceed,
                            Message = learningRegisResponse.Message,
                            Data = enrichedRegistration
                        };
                    }
                }

                return learningRegisResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enriching single learning registration {learningRegisId} with payment info");
                return learningRegisResponse;
            }
        }
    }
}