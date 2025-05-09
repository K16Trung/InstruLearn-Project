using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.EntityFrameworkCore;
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

        public PaymentInfoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learningRegis.LearnerId);
                if (learner == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy học viên."
                    };
                }

                string teacherName = "Chưa có";
                if (learningRegis.TeacherId.HasValue)
                {
                    var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(learningRegis.TeacherId.Value);
                    if (teacher != null)
                    {
                        teacherName = teacher.Fullname;
                    }
                }

                decimal totalPrice = learningRegis.Price ?? 0;
                decimal firstPaymentAmount = Math.Round(totalPrice * 0.4m, 0);
                decimal secondPaymentAmount = Math.Round(totalPrice * 0.6m, 0);

                var firstPaymentCompleted = false;
                var secondPaymentCompleted = false;
                string firstPaymentStatus = "Chưa thanh toán";
                string secondPaymentStatus = "Chưa thanh toán";
                DateTime? firstPaymentDate = null;
                DateTime? secondPaymentDate = null;

                var payments = await _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => p.PaymentFor == PaymentFor.LearningRegistration && p.PaymentId == learningRegisId && p.Status == PaymentStatus.Completed)
                    .ToListAsync();

                if (payments != null && payments.Any())
                {
                    foreach (var payment in payments)
                    {
                        var transaction = await _unitOfWork.WalletTransactionRepository
                            .GetTransactionWithWalletAsync(payment.TransactionId);

                        if (transaction != null)
                        {
                            if (Math.Abs(payment.AmountPaid - firstPaymentAmount) < 0.1m && !firstPaymentCompleted)
                            {
                                firstPaymentCompleted = true;
                                firstPaymentStatus = "Đã thanh toán";
                                firstPaymentDate = transaction.TransactionDate;
                            }
                            else if (Math.Abs(payment.AmountPaid - secondPaymentAmount) < 0.1m && !secondPaymentCompleted)
                            {
                                secondPaymentCompleted = true;
                                secondPaymentStatus = "Đã thanh toán";
                                secondPaymentDate = transaction.TransactionDate;
                            }
                        }
                    }
                }

                int? firstPaymentRemainingDays = null;
                int? secondPaymentRemainingDays = null;
                DateTime? firstPaymentDeadline = null;
                DateTime? secondPaymentDeadline = null;

                if (learningRegis.Status == LearningRegis.Accepted && learningRegis.PaymentDeadline.HasValue)
                {
                    firstPaymentDeadline = learningRegis.PaymentDeadline;
                    firstPaymentRemainingDays = (int)Math.Max(0, (firstPaymentDeadline.Value - DateTime.Now).TotalDays);

                    if (!firstPaymentCompleted && DateTime.Now > firstPaymentDeadline)
                    {
                        firstPaymentStatus = "Quá hạn";

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
                            catch
                            {
                                await transaction.RollbackAsync();
                            }
                        }
                    }
                }

                if ((learningRegis.Status == LearningRegis.FourtyFeedbackDone || learningRegis.Status == LearningRegis.Fourty) &&
                    learningRegis.PaymentDeadline.HasValue)
                {
                    secondPaymentDeadline = learningRegis.PaymentDeadline;
                    secondPaymentRemainingDays = (int)Math.Max(0, (secondPaymentDeadline.Value - DateTime.Now).TotalDays);

                    if (!secondPaymentCompleted && DateTime.Now > secondPaymentDeadline)
                    {
                        secondPaymentStatus = "Quá hạn";

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
                            catch
                            {
                                await transaction.RollbackAsync();
                            }
                        }
                    }
                }

                var firstPaymentPeriod = new
                {
                    PaymentPercent = 40,
                    PaymentAmount = firstPaymentAmount,
                    PaymentStatus = firstPaymentStatus,
                    PaymentDeadline = firstPaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentDate = firstPaymentDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    RemainingDays = firstPaymentRemainingDays
                };

                var secondPaymentPeriod = new
                {
                    PaymentPercent = 60,
                    PaymentAmount = secondPaymentAmount,
                    PaymentStatus = secondPaymentStatus,
                    PaymentDeadline = secondPaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss"),
                    PaymentDate = secondPaymentDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    RemainingDays = secondPaymentRemainingDays
                };

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Thông tin thanh toán đã được lấy thành công.",
                    Data = new
                    {
                        firstPaymentPeriod = firstPaymentPeriod,
                        secondPaymentPeriod = secondPaymentPeriod
                    }
                };
            }
            catch (Exception ex)
            {
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
                Console.WriteLine($"Error enriching learning registrations: {ex.Message}");
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
                Console.WriteLine($"Error enriching learning registration: {ex.Message}");
                return learningRegisResponse;
            }
        }
    }
}
