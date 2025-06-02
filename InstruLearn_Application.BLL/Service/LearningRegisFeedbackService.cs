using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.FeedbackSummary;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedback;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackAnswer;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackOption;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackQuestion;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningRegisFeedbackService : ILearningRegisFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public LearningRegisFeedbackService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<List<LearningRegisFeedbackDTO>> GetAllFeedbackAsync()
        {
            try
            {
                var feedbacks = await _unitOfWork.LearningRegisFeedbackRepository.GetAllWithDetailsAsync();

                return feedbacks.Select(MapToFeedbackDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all feedback: {ex.Message}", ex);
            }
        }


        public async Task<ResponseDTO> CreateQuestionAsync(LearningRegisFeedbackQuestionDTO questionDTO)
        {
            var question = new LearningRegisFeedbackQuestion
            {
                QuestionText = questionDTO.QuestionText,
                DisplayOrder = questionDTO.DisplayOrder,
                IsRequired = questionDTO.IsRequired,
                IsActive = true,
                Options = new List<LearningRegisFeedbackOption>()
            };

            await _unitOfWork.LearningRegisFeedbackQuestionRepository.AddAsync(question);
            await _unitOfWork.SaveChangeAsync();

            if (questionDTO.Options != null)
            {
                foreach (var optionDTO in questionDTO.Options)
                {
                    var option = new LearningRegisFeedbackOption
                    {
                        QuestionId = question.QuestionId,
                        OptionText = optionDTO.OptionText
                    };
                    await _unitOfWork.LearningRegisFeedbackOptionRepository.AddAsync(option);
                }
                await _unitOfWork.SaveChangeAsync();
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Tạo câu hỏi đánh giá thành công"
            };
        }


        public async Task<ResponseDTO> UpdateQuestionAsync(int questionId, LearningRegisFeedbackQuestionDTO questionDTO)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetQuestionWithOptionsAsync(questionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }

            question.QuestionText = questionDTO.QuestionText;
            question.DisplayOrder = questionDTO.DisplayOrder;
            question.IsRequired = questionDTO.IsRequired;

            await _unitOfWork.LearningRegisFeedbackQuestionRepository.UpdateAsync(question);

            if (questionDTO.Options != null)
            {
                var existingOptions = await _unitOfWork.LearningRegisFeedbackOptionRepository.GetOptionsByQuestionIdAsync(questionId);
                var existingOptionIds = existingOptions.Select(o => o.OptionId).ToList();
                var updatedOptionIds = questionDTO.Options.Where(o => o.OptionId > 0).Select(o => o.OptionId).ToList();

                foreach (var optionId in existingOptionIds)
                {
                    if (!updatedOptionIds.Contains(optionId))
                    {
                        await _unitOfWork.LearningRegisFeedbackOptionRepository.DeleteAsync(optionId);
                    }
                }

                foreach (var optionDTO in questionDTO.Options)
                {
                    if (optionDTO.OptionId > 0)
                    {
                        var option = existingOptions.FirstOrDefault(o => o.OptionId == optionDTO.OptionId);
                        if (option != null)
                        {
                            option.OptionText = optionDTO.OptionText;
                            await _unitOfWork.LearningRegisFeedbackOptionRepository.UpdateAsync(option);
                        }
                    }
                    else
                    {
                        var newOption = new LearningRegisFeedbackOption
                        {
                            QuestionId = questionId,
                            OptionText = optionDTO.OptionText,
                        };
                        await _unitOfWork.LearningRegisFeedbackOptionRepository.AddAsync(newOption);
                    }
                }
            }

            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Cập nhật câu hỏi đánh giá thành công"
            };
        }

        public async Task<ResponseDTO> DeleteQuestionAsync(int questionId)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }

            var options = await _unitOfWork.LearningRegisFeedbackOptionRepository.GetOptionsByQuestionIdAsync(questionId);
            foreach (var option in options)
            {
                await _unitOfWork.LearningRegisFeedbackOptionRepository.DeleteAsync(option.OptionId);
            }

            await _unitOfWork.LearningRegisFeedbackQuestionRepository.DeleteAsync(questionId);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Xóa câu hỏi đánh giá thành công"
            };
        }

        public async Task<ResponseDTO> ActivateQuestionAsync(int questionId)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }

            question.IsActive = true;
            await _unitOfWork.LearningRegisFeedbackQuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Kích hoạt câu hỏi thành công"
            };
        }

        public async Task<ResponseDTO> DeactivateQuestionAsync(int questionId)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi"
                };
            }

            question.IsActive = false;
            await _unitOfWork.LearningRegisFeedbackQuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Vô hiệu hóa câu hỏi thành công"
            };
        }

        public async Task<LearningRegisFeedbackQuestionDTO> GetQuestionAsync(int questionId)
        {
            var question = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetQuestionWithOptionsAsync(questionId);
            if (question == null)
                return null;

            return MapToQuestionDTO(question);
        }

        public async Task<List<LearningRegisFeedbackQuestionDTO>> GetAllActiveQuestionsAsync()
        {
            var questions = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetActiveQuestionsWithOptionsAsync();
            return questions.Select(MapToQuestionDTO).ToList();
        }

        public async Task<ResponseDTO> SubmitFeedbackAsync(CreateLearningRegisFeedbackDTO createDTO)
        {
            var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(createDTO.LearningRegistrationId);
            if (learningRegis == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy đăng ký học"
                };
            }

            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(createDTO.LearnerId);
            if (learner == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy học viên"
                };
            }

            var existingFeedback = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbackByRegistrationIdAsync(createDTO.LearningRegistrationId);

            if (existingFeedback != null && existingFeedback.Status == FeedbackStatus.Completed)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Đã tồn tại đánh giá đã hoàn thành cho đăng ký học này"
                };
            }

            var activeQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetActiveQuestionsWithOptionsAsync();
            if (activeQuestions == null || !activeQuestions.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy câu hỏi đánh giá nào đang hoạt động"
                };
            }

            var requiredQuestionIds = activeQuestions.Where(q => q.IsRequired).Select(q => q.QuestionId).ToList();
            var answeredQuestionIds = createDTO.Answers.Select(a => a.QuestionId).ToList();

            var missingRequiredQuestions = requiredQuestionIds.Except(answeredQuestionIds).ToList();
            if (missingRequiredQuestions.Any())
            {
                var missingQuestionText = string.Join(", ", activeQuestions
                    .Where(q => missingRequiredQuestions.Contains(q.QuestionId))
                    .Select(q => q.QuestionText));

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Vui lòng trả lời các câu hỏi bắt buộc: {missingQuestionText}"
                };
            }

            try
            {
                int feedbackId;

                if (existingFeedback != null)
                {
                    existingFeedback.AdditionalComments = createDTO.AdditionalComments;
                    existingFeedback.TeacherChangeReason = createDTO.TeacherChangeReason;
                    existingFeedback.CompletedAt = DateTime.Now;
                    existingFeedback.Status = FeedbackStatus.Completed;

                    var existingAnswers = await _unitOfWork.LearningRegisFeedbackAnswerRepository
                        .GetAnswersByFeedbackIdAsync(existingFeedback.FeedbackId);

                    foreach (var answer in existingAnswers)
                    {
                        await _unitOfWork.LearningRegisFeedbackAnswerRepository.DeleteAsync(answer.AnswerId);
                    }

                    foreach (var answerDTO in createDTO.Answers)
                    {
                        var question = activeQuestions.FirstOrDefault(q => q.QuestionId == answerDTO.QuestionId);
                        if (question == null)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Câu hỏi không hợp lệ (ID: {answerDTO.QuestionId})"
                            };
                        }

                        if (question.Options == null || !question.Options.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Câu hỏi (ID: {answerDTO.QuestionId}) không có lựa chọn nào"
                            };
                        }

                        var option = question.Options.FirstOrDefault(o => o.OptionId == answerDTO.SelectedOptionId);
                        if (option == null)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Lựa chọn không hợp lệ cho câu hỏi (ID: {answerDTO.QuestionId})"
                            };
                        }

                        var newAnswer = new LearningRegisFeedbackAnswer
                        {
                            FeedbackId = existingFeedback.FeedbackId,
                            QuestionId = answerDTO.QuestionId,
                            SelectedOptionId = answerDTO.SelectedOptionId,
                        };

                        await _unitOfWork.LearningRegisFeedbackAnswerRepository.AddAsync(newAnswer);
                    }

                    await _unitOfWork.LearningRegisFeedbackRepository.UpdateAsync(existingFeedback);
                    await _unitOfWork.SaveChangeAsync();

                    feedbackId = existingFeedback.FeedbackId;
                }
                else
                {
                    var feedback = new LearningRegisFeedback
                    {
                        LearningRegistrationId = createDTO.LearningRegistrationId,
                        LearnerId = createDTO.LearnerId,
                        AdditionalComments = createDTO.AdditionalComments,
                        TeacherChangeReason = createDTO.TeacherChangeReason,
                        CreatedAt = DateTime.Now,
                        CompletedAt = DateTime.Now,
                        Status = FeedbackStatus.Completed,
                        Answers = new List<LearningRegisFeedbackAnswer>()
                    };

                    var relatedNotifications = await _unitOfWork.StaffNotificationRepository
                        .GetQuery()
                        .Where(n => n.LearningRegisId == createDTO.LearningRegistrationId &&
                                    n.Type == NotificationType.FeedbackRequired &&
                                    n.Status != NotificationStatus.Resolved)
                        .ToListAsync();

                    if (relatedNotifications.Any())
                    {
                        foreach (var notification in relatedNotifications)
                        {
                            notification.Status = NotificationStatus.Resolved;
                            await _unitOfWork.StaffNotificationRepository.UpdateAsync(notification);
                        }
                    }

                    foreach (var answerDTO in createDTO.Answers)
                    {
                        var question = activeQuestions.FirstOrDefault(q => q.QuestionId == answerDTO.QuestionId);
                        if (question == null)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Câu hỏi không hợp lệ (ID: {answerDTO.QuestionId})"
                            };
                        }

                        if (question.Options == null || !question.Options.Any())
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Câu hỏi (ID: {answerDTO.QuestionId}) không có lựa chọn nào"
                            };
                        }

                        var option = question.Options.FirstOrDefault(o => o.OptionId == answerDTO.SelectedOptionId);
                        if (option == null)
                        {
                            return new ResponseDTO
                            {
                                IsSucceed = false,
                                Message = $"Lựa chọn không hợp lệ cho câu hỏi (ID: {answerDTO.QuestionId})"
                            };
                        }

                        feedback.Answers.Add(new LearningRegisFeedbackAnswer
                        {
                            QuestionId = answerDTO.QuestionId,
                            SelectedOptionId = answerDTO.SelectedOptionId,
                        });
                    }

                    await _unitOfWork.LearningRegisFeedbackRepository.AddAsync(feedback);
                    await _unitOfWork.SaveChangeAsync();

                    var newFeedback = await _unitOfWork.LearningRegisFeedbackRepository
                        .GetFeedbackByRegistrationIdAsync(createDTO.LearningRegistrationId);
                    feedbackId = newFeedback.FeedbackId;
                }

                if (createDTO.ContinueStudying && createDTO.ChangeTeacher)
                {
                    if (learningRegis.Status == LearningRegis.Fourty)
                    {
                        learningRegis.Status = LearningRegis.FourtyFeedbackDone;
                        learningRegis.PaymentDeadline = null;
                        learningRegis.ChangeTeacherRequested = true;
                        learningRegis.TeacherChangeProcessed = false;
                        await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    var teacherChangeNotification = new StaffNotification
                    {
                        Title = "Yêu cầu thay đổi giáo viên",
                        Message = $"Học viên {learner.FullName} (ID: {learner.LearnerId}) muốn tiếp tục học nhưng thay đổi giáo viên cho đăng ký học ID: {learningRegis.LearningRegisId}.Lý do: {createDTO.TeacherChangeReason ?? "Không có lý do cụ thể"}",
                        LearningRegisId = learningRegis.LearningRegisId,
                        LearnerId = learner.LearnerId,
                        CreatedAt = DateTime.Now,
                        Status = NotificationStatus.Unread,
                        Type = NotificationType.TeacherChangeRequest
                    };

                    await _unitOfWork.StaffNotificationRepository.AddAsync(teacherChangeNotification);
                    await _unitOfWork.SaveChangeAsync();

                    if (learner != null && !string.IsNullOrEmpty(learner.AccountId))
                    {
                        var learnerAccount = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
                        if (learnerAccount != null && !string.IsNullOrEmpty(learnerAccount.Email))
                        {
                            try
                            {
                                decimal remainingPayment = learningRegis.Price.HasValue ? learningRegis.Price.Value * 0.6m : 0;

                                string subject = "Thanh toán học phí còn lại - Hạn chót 1 ngày";
                                string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                        <h2 style='color: #333;'>Xin chào {learner.FullName},</h2>
                        
                        <p>Cảm ơn bạn đã hoàn thành đánh giá và quyết định tiếp tục học với giáo viên khác.</p>
                        
                        <p>Nhân viên của chúng tôi sẽ liên hệ với bạn để sắp xếp giáo viên mới.</p>
                        
                        <p><strong>Sau khi giáo viên mới được chỉ định, bạn sẽ nhận được thông báo và hạn thanh toán cho 60% học phí còn lại.</strong></p>
                        
                        <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                            <h3 style='margin-top: 0; color: #333;'>Thông tin thanh toán:</h3>
                            <p><strong>Số tiền cần thanh toán:</strong> {remainingPayment:N0} VND</p>
                            <p><strong>ID đăng ký học:</strong> {learningRegis.LearningRegisId}</p>
                        </div>
                        
                        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
                        
                        <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                    </div>
                </body>
                </html>";

                                await _emailService.SendEmailAsync(learnerAccount.Email, subject, body, true);
                            }
                            catch (Exception emailEx)
                            {
                            }
                        }
                    }

                    string paymentDeadlineStr = learningRegis.PaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Gửi đánh giá thành công. Yêu cầu thay đổi giáo viên của bạn đã được ghi nhận. Đăng ký học của bạn đã sẵn sàng cho khoản thanh toán 60% còn lại để tiếp tục học.",
                        Data = new
                        {
                            FeedbackId = feedbackId,
                            LearningRegisId = learningRegis.LearningRegisId,
                            Status = learningRegis.Status.ToString(),
                            RemainingPayment = learningRegis.Price.HasValue ? learningRegis.Price.Value * 0.6m : 0,
                            TeacherChangeRequested = true,
                            PaymentDeadline = (string)null
                        }
                    };
                }

                if (createDTO.ContinueStudying)
                {
                    if (learningRegis.Status == LearningRegis.Fourty)
                    {
                        learningRegis.Status = LearningRegis.FourtyFeedbackDone;
                        learningRegis.PaymentDeadline = DateTime.Now.AddDays(1);
                        learningRegis.ChangeTeacherRequested = false;
                        learningRegis.TeacherChangeProcessed = false;
                        await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                        await _unitOfWork.SaveChangeAsync();

                        if (learner != null && !string.IsNullOrEmpty(learner.AccountId))
                        {
                            var learnerAccount = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
                            if (learnerAccount != null && !string.IsNullOrEmpty(learnerAccount.Email))
                            {
                                try
                                {
                                    decimal remainingPayment = learningRegis.Price.HasValue ? learningRegis.Price.Value * 0.6m : 0;
                                    string deadlineFormatted = learningRegis.PaymentDeadline?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

                                    string subject = "Thanh toán học phí còn lại - Hạn chót 1 ngày";
                                    string body = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                                    <h2 style='color: #333;'>Xin chào {learner.FullName},</h2>
                                    
                                    <p>Cảm ơn bạn đã hoàn thành đánh giá và quyết định tiếp tục học.</p>
                                    
                                    <p><strong>Bạn có 1 ngày để thanh toán 60% học phí còn lại.</strong></p>
                                    
                                    <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                                        <h3 style='margin-top: 0; color: #333;'>Thông tin thanh toán:</h3>
                                        <p><strong>Số tiền cần thanh toán:</strong> {remainingPayment:N0} VND</p>
                                        <p><strong>Hạn thanh toán:</strong> {deadlineFormatted}</p>
                                        <p><strong>ID đăng ký học:</strong> {learningRegis.LearningRegisId}</p>
                                    </div>
                                    
                                    <p>Nếu không thanh toán trước hạn, đăng ký học của bạn sẽ bị hủy tự động.</p>
                                    
                                    <div style='background-color: #4CAF50; text-align: center; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                                        <a href='https://instrulearn.com/payment/{learningRegis.LearningRegisId}' style='color: white; text-decoration: none; font-weight: bold; font-size: 16px;'>
                                            Thanh Toán Ngay
                                        </a>
                                    </div>
                                    
                                    <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
                                    
                                    <p>Trân trọng,<br>Đội ngũ InstruLearn</p>
                                </div>
                            </body>
                            </html>";

                                    await _emailService.SendEmailAsync(learnerAccount.Email, subject, body, true);
                                }
                                catch (Exception emailEx)
                                {
                                }
                            }
                        }
                    }

                    string paymentDeadlineStr = learningRegis.PaymentDeadline?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Gửi đánh giá thành công. Đăng ký học của bạn đã sẵn sàng cho khoản thanh toán 60% còn lại để tiếp tục học.",
                        Data = new
                        {
                            FeedbackId = feedbackId,
                            LearningRegisId = learningRegis.LearningRegisId,
                            Status = learningRegis.Status.ToString(),
                            RemainingPayment = learningRegis.Price.HasValue ? learningRegis.Price.Value * 0.6m : 0,
                            PaymentDeadline = paymentDeadlineStr,
                            PaymentDeadlineUnix = learningRegis.PaymentDeadline.HasValue ?
                        new DateTimeOffset(learningRegis.PaymentDeadline.Value).ToUnixTimeSeconds() : 0
                        }
                    };
                }
                else
                {
                    learningRegis.Status = LearningRegis.Cancelled;
                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);

                    var learningRegisSchedules = await _unitOfWork.ScheduleRepository.GetSchedulesByLearningRegisIdAsync(learningRegis.LearningRegisId);

                    int? teacherId = null;
                    if (learningRegisSchedules.Any() && learningRegisSchedules.First().TeacherId.HasValue)
                    {
                        teacherId = learningRegisSchedules.First().TeacherId;
                    }
                    else if (learningRegis.TeacherId.HasValue)
                    {
                        teacherId = learningRegis.TeacherId;
                    }

                    foreach (var schedule in learningRegisSchedules)
                    {
                        await _unitOfWork.ScheduleRepository.DeleteAsync(schedule.ScheduleId);
                    }

                    await _unitOfWork.SaveChangeAsync();

                    if (teacherId.HasValue)
                    {
                        var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId.Value);
                        if (teacher != null && !string.IsNullOrEmpty(teacher.AccountId))
                        {
                            var teacherAccount = await _unitOfWork.AccountRepository.GetByIdAsync(teacher.AccountId);
                            if (teacherAccount != null && !string.IsNullOrEmpty(teacherAccount.Email))
                            {
                                try
                                {
                                    string subject = "Learning Registration Cancelled";
                                    string body = $@"
                            <html>
                            <body>
                                <h2>Thông báo hủy đăng ký học</h2>
                                <p>Xin chào {teacher.Fullname},</p>
                                <p>Học viên {learner.FullName} đã quyết định không tiếp tục khóa học.</p>
                                <p>Tất cả lịch học liên quan đến đăng ký học này đã bị hủy.</p>
                                <p>Vui lòng kiểm tra hệ thống để cập nhật lịch dạy của bạn.</p>
                                <p>Trân trọng,<br/>The InstruLearn Team</p>
                            </body>
                            </html>";

                                    await _emailService.SendEmailAsync(teacherAccount.Email, subject, body);
                                }
                                catch (Exception emailEx)
                                {
                                }
                            }
                        }
                    }

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Gửi đánh giá thành công. Đăng ký học của bạn đã bị hủy theo yêu cầu và tất cả lịch học đã được xóa.",
                        Data = new
                        {
                            FeedbackId = feedbackId,
                            LearningRegisId = learningRegis.LearningRegisId,
                            Status = learningRegis.Status.ToString(),
                            SchedulesRemoved = learningRegisSchedules.Count
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lưu đánh giá: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateFeedbackAsync(int feedbackId, UpdateLearningRegisFeedbackDTO updateDTO)
        {
            var feedback = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbackWithDetailsAsync(feedbackId);
            if (feedback == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy đánh giá"
                };
            }

            feedback.AdditionalComments = updateDTO.AdditionalComments;
            feedback.CompletedAt = DateTime.Now;

            var activeQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetActiveQuestionsWithOptionsAsync();

            var existingAnswers = await _unitOfWork.LearningRegisFeedbackAnswerRepository.GetAnswersByFeedbackIdAsync(feedbackId);
            foreach (var answer in existingAnswers)
            {
                await _unitOfWork.LearningRegisFeedbackAnswerRepository.DeleteAsync(answer.AnswerId);
            }

            foreach (var answerDTO in updateDTO.Answers)
            {
                var question = activeQuestions.FirstOrDefault(q => q.QuestionId == answerDTO.QuestionId);
                if (question == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Câu hỏi không hợp lệ (ID: {answerDTO.QuestionId})"
                    };
                }

                var option = question.Options.FirstOrDefault(o => o.OptionId == answerDTO.SelectedOptionId);
                if (option == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Lựa chọn không hợp lệ cho câu hỏi (ID: {answerDTO.QuestionId})"
                    };
                }

                var newAnswer = new LearningRegisFeedbackAnswer
                {
                    FeedbackId = feedbackId,
                    QuestionId = answerDTO.QuestionId,
                    SelectedOptionId = answerDTO.SelectedOptionId,
                };

                await _unitOfWork.LearningRegisFeedbackAnswerRepository.AddAsync(newAnswer);
            }

            await _unitOfWork.LearningRegisFeedbackRepository.UpdateAsync(feedback);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Cập nhật đánh giá thành công"
            };
        }

        public async Task<ResponseDTO> DeleteFeedbackAsync(int feedbackId)
        {
            var feedback = await _unitOfWork.LearningRegisFeedbackRepository.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy đánh giá"
                };
            }

            var answers = await _unitOfWork.LearningRegisFeedbackAnswerRepository.GetAnswersByFeedbackIdAsync(feedbackId);
            foreach (var answer in answers)
            {
                await _unitOfWork.LearningRegisFeedbackAnswerRepository.DeleteAsync(answer.AnswerId);
            }

            await _unitOfWork.LearningRegisFeedbackRepository.DeleteAsync(feedbackId);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Xóa đánh giá thành công"
            };
        }

        public async Task<LearningRegisFeedbackDTO> GetFeedbackByIdAsync(int feedbackId)
        {
            var feedback = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbackWithDetailsAsync(feedbackId);
            if (feedback == null)
                return null;

            return MapToFeedbackDTO(feedback);
        }

        public async Task<LearningRegisFeedbackDTO> GetFeedbackByRegistrationIdAsync(int registrationId)
        {
            var feedback = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbackByRegistrationIdAsync(registrationId);
            if (feedback == null)
                return null;

            return MapToFeedbackDTO(feedback);
        }

        public async Task<List<LearningRegisFeedbackDTO>> GetFeedbacksByTeacherIdAsync(int teacherId)
        {
            var feedbacks = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbacksByTeacherIdAsync(teacherId);
            return feedbacks.Select(MapToFeedbackDTO).ToList();
        }

        public async Task<List<LearningRegisFeedbackDTO>> GetFeedbacksByLearnerIdAsync(int learnerId)
        {
            var feedbacks = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbacksByLearnerIdAsync(learnerId);
            return feedbacks.Select(MapToFeedbackDTO).ToList();
        }

        public async Task<TeacherFeedbackSummaryDTO> GetTeacherFeedbackSummaryAsync(int teacherId)
        {
            var feedbacks = await _unitOfWork.LearningRegisFeedbackRepository.GetFeedbacksByTeacherIdAsync(teacherId);
            if (feedbacks == null || !feedbacks.Any())
            {
                var teachers = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                return new TeacherFeedbackSummaryDTO
                {
                    TeacherId = teacherId,
                    TeacherName = teachers?.Fullname ?? "Unknown",
                    TotalFeedbacks = 0,
                    OverallAverageRating = 0,
                    CategoryAverages = new Dictionary<string, double>(),
                    QuestionSummaries = new List<QuestionSummaryDTO>()
                };
            }

            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);

            var allQuestions = await _unitOfWork.LearningRegisFeedbackQuestionRepository.GetAllAsync();

            double overallRatingSum = 0;
            int overallRatingCount = 0;

            var categoryRatings = new Dictionary<string, List<double>>();
            const string defaultCategory = "General";

            var questionSummaries = new Dictionary<int, QuestionSummaryDTO>();

            foreach (var feedback in feedbacks)
            {
                foreach (var answer in feedback.Answers)
                {
                    var questionId = answer.QuestionId;
                    int optionValue = GetOptionPositionAsValue(answer.SelectedOptionId);
                    var question = answer.Question ?? allQuestions.FirstOrDefault(q => q.QuestionId == questionId);

                    if (question == null)
                        continue;

                    overallRatingSum += optionValue;
                    overallRatingCount++;

                    if (!categoryRatings.ContainsKey(defaultCategory))
                    {
                        categoryRatings[defaultCategory] = new List<double>();
                    }
                    categoryRatings[defaultCategory].Add(optionValue);

                    if (!questionSummaries.ContainsKey(questionId))
                    {
                        questionSummaries[questionId] = new QuestionSummaryDTO
                        {
                            QuestionId = questionId,
                            QuestionText = question.QuestionText,
                            Category = defaultCategory,
                            AverageRating = 0,
                            OptionCounts = new List<OptionCountDTO>()
                        };
                    }

                    var optionId = answer.SelectedOptionId;
                    var option = answer.SelectedOption;

                    var existingOptionCount = questionSummaries[questionId].OptionCounts
                        .FirstOrDefault(o => o.OptionId == optionId);

                    if (existingOptionCount == null)
                    {
                        questionSummaries[questionId].OptionCounts.Add(new OptionCountDTO
                        {
                            OptionId = optionId,
                            OptionText = option?.OptionText ?? "Unknown",
                            Count = 1,
                            Percentage = 0
                        });
                    }
                    else
                    {
                        existingOptionCount.Count++;
                    }
                }
            }

            var overallAverage = overallRatingCount > 0 ? overallRatingSum / overallRatingCount : 0;

            var categoryAverages = new Dictionary<string, double>();
            foreach (var category in categoryRatings.Keys)
            {
                categoryAverages[category] = categoryRatings[category].Average();
            }

            foreach (var questionId in questionSummaries.Keys)
            {
                var summary = questionSummaries[questionId];

                int totalResponses = summary.OptionCounts.Sum(o => o.Count);
                foreach (var option in summary.OptionCounts)
                {
                    option.Percentage = totalResponses > 0 ? (double)option.Count / totalResponses * 100 : 0;
                }

                summary.AverageRating = totalResponses > 0
                    ? summary.OptionCounts.Sum(o => o.Count * GetOptionPositionAsValue(o.OptionId)) / totalResponses
                    : 0;
            }

            return new TeacherFeedbackSummaryDTO
            {
                TeacherId = teacherId,
                TeacherName = teacher?.Fullname ?? "Unknown",
                TotalFeedbacks = feedbacks.Count,
                OverallAverageRating = overallAverage,
                CategoryAverages = categoryAverages,
                QuestionSummaries = questionSummaries.Values.ToList()
            };
        }

        private int GetOptionPositionAsValue(int optionId)
        {
            var option = _unitOfWork.LearningRegisFeedbackOptionRepository.GetByIdAsync(optionId).Result;
            if (option == null)
                return 0;

            var options = _unitOfWork.LearningRegisFeedbackOptionRepository.GetOptionsByQuestionIdAsync(option.QuestionId).Result;
            if (options == null || !options.Any())
                return 0;

            var sortedOptions = options.OrderBy(o => o.OptionId).ToList();

            int position = sortedOptions.FindIndex(o => o.OptionId == optionId) + 1;

            return position > 0 ? position : 1;
        }

        private LearningRegisFeedbackDTO MapToFeedbackDTO(LearningRegisFeedback feedback)
        {
            if (feedback == null)
                return null;

            var feedbackDTO = _mapper.Map<LearningRegisFeedbackDTO>(feedback);

            double totalRating = 0;
            int ratingCount = 0;

            if (feedback.Answers != null)
            {
                foreach (var answer in feedback.Answers)
                {
                    int value = GetOptionPositionAsValue(answer.SelectedOptionId);
                    totalRating += value;
                    ratingCount++;
                }
            }

            feedbackDTO.AverageRating = ratingCount > 0 ? totalRating / ratingCount : 0;

            return feedbackDTO;
        }

        private LearningRegisFeedbackQuestionDTO MapToQuestionDTO(LearningRegisFeedbackQuestion question)
        {
            return _mapper.Map<LearningRegisFeedbackQuestionDTO>(question);
        }
    }
}
