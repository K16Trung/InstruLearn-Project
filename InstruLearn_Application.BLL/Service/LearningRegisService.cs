using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Enum;
using System.Transactions;
using Microsoft.Extensions.Logging;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using InstruLearn_Application.Model.Models.DTO.LearnerClass;
using Microsoft.EntityFrameworkCore;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningRegisService : ILearningRegisService
    {
        private readonly ILearningRegisRepository _learningRegisRepository;
        private readonly ILogger<LearningRegisService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LearningRegisService(ILearningRegisRepository learningRegisRepository, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LearningRegisService> logger)
        {
            _learningRegisRepository = learningRegisRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ResponseDTO> GetAllLearningRegisAsync()
        {
            var allRegistrations = await _learningRegisRepository.GetAllAsync();
            var allDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(allRegistrations);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "All learning registrations retrieved successfully.",
                Data = allDtos
            };
        }

        public async Task<ResponseDTO> GetLearningRegisByIdAsync(int learningRegisId)
        {
            var registration = await _learningRegisRepository.GetByIdAsync(learningRegisId);
            if (registration == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning registration not found.",
                    Data = null
                };
            }

            var dto = _mapper.Map<OneOnOneRegisDTO>(registration);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning registration retrieved successfully.",
                Data = dto
            };
        }

        public async Task<ResponseDTO> CreateLearningRegisAsync(CreateLearningRegisDTO createLearningRegisDTO)
        {
            try
            {
                _logger.LogInformation("Starting learning registration process.");

                // Start EF Core transaction instead of TransactionScope
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == createLearningRegisDTO.LearnerId);

                    if (wallet == null)
                    {
                        _logger.LogWarning($"Wallet not found for learnerId: {createLearningRegisDTO.LearnerId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Wallet not found for the learner."
                        };
                    }

                    _logger.LogInformation($"Wallet found for learnerId: {createLearningRegisDTO.LearnerId}, balance: {wallet.Balance}");

                    if (wallet.Balance < 50000)
                    {
                        _logger.LogWarning($"Insufficient balance for learnerId: {createLearningRegisDTO.LearnerId}. Current balance: {wallet.Balance}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Insufficient balance in the wallet."
                        };
                    }

                    // Deduct the balance
                    wallet.Balance -= 50000;
                    await _unitOfWork.WalletRepository.UpdateAsync(wallet);

                    // Validate learning duration (30, 60, 90 or 120 minutes)
                    if (createLearningRegisDTO.TimeLearning != 45 && createLearningRegisDTO.TimeLearning != 60 && createLearningRegisDTO.TimeLearning != 90 && createLearningRegisDTO.TimeLearning != 120)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Invalid learning duration. Please select 30, 60, 90 or 120 minutes."
                        };
                    }

                    // Map DTO to entity
                    var learningRegis = _mapper.Map<Learning_Registration>(createLearningRegisDTO);
                    learningRegis.Status = LearningRegis.Pending;

                    // Add learning registration
                    await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    // Add learning registration days
                    if (createLearningRegisDTO.LearningDays != null && createLearningRegisDTO.LearningDays.Any())
                    {
                        var learningDays = createLearningRegisDTO.LearningDays.Select(day => new LearningRegistrationDay
                        {
                            LearningRegisId = learningRegis.LearningRegisId,
                            DayOfWeek = day
                        }).ToList();

                        await _unitOfWork.LearningRegisDayRepository.AddRangeAsync(learningDays);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    // Create Test_Result and associate it with the Learning_Registration
                    var testResult = new Test_Result
                    {
                        LearnerId = createLearningRegisDTO.LearnerId,
                        TeacherId = createLearningRegisDTO.TeacherId ?? 0,
                        MajorId = createLearningRegisDTO.MajorId,
                        LearningRegisId = learningRegis.LearningRegisId,
                        ResultType = TestResultType.OneOnOne,
                        Status = TestResultStatus.Pending,        
                        LearningRegistration = learningRegis     
                    };

                    // Add the Video URL in Test_Result (stored indirectly)
                    testResult.LearningRegistration.VideoUrl = createLearningRegisDTO.VideoUrl;

                    await _unitOfWork.TestResultRepository.AddAsync(testResult);
                    await _unitOfWork.SaveChangeAsync();

                    // Create a wallet transaction
                    var walletTransaction = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        WalletId = wallet.WalletId,
                        Amount = 50000,
                        TransactionType = TransactionType.Payment,
                        Status = Model.Enum.TransactionStatus.Complete,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);
                    await _unitOfWork.SaveChangeAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();

                    _logger.LogInformation("Learning registration added successfully. Wallet balance updated.");

                    // Calculate TimeEnd dynamically (without saving it)
                    var timeEnd = learningRegis.TimeStart.AddMinutes(createLearningRegisDTO.TimeLearning);
                    var timeEndFormatted = timeEnd.ToString("HH:mm");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Learning Registration added successfully. Wallet balance updated. Status set to Pending."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing learning registration.");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


        public async Task<ResponseDTO> DeleteLearningRegisAsync(int learningRegisId)
        {
            var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
            if (learningRegis == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning Registration not found."
                };
            }
            await _unitOfWork.LearningRegisRepository.DeleteAsync(learningRegisId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration deleted successfully."
            };
        }

        public async Task<ResponseDTO> GetAllPendingRegistrationsAsync()
        {
            var pendingRegistrations = await _learningRegisRepository.GetPendingRegistrationsAsync();
            var pendingDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(pendingRegistrations);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Pending learning registrations retrieved successfully.",
                Data = pendingDtos
            };
        }

        public async Task<ResponseDTO> GetRegistrationsByLearnerIdAsync(int learnerId)
        {
            var registrations = await _learningRegisRepository.GetRegistrationsByLearnerIdAsync(learnerId);
            var registrationDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(registrations);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = $"All registrations for Learner ID {learnerId} retrieved successfully.",
                Data = registrationDtos
            };
        }

        public async Task<ResponseDTO> UpdateLearningRegisStatusAsync(UpdateLearningRegisDTO updateDTO)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(updateDTO.LearningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found."
                    };
                }

                // Fetch LevelAssigned to get the correct LevelPrice
                var levelAssigned = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(updateDTO.LevelId);
                if (levelAssigned == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Level Assigned not found."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                // Map other properties (excluding Price)
                _mapper.Map(updateDTO, learningRegis);

                // Update the Price based on LevelAssigned
                learningRegis.Price = levelAssigned.LevelPrice * learningRegis.NumberOfSession;

                // Manually update status
                learningRegis.Status = LearningRegis.Accepted;

                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                await _unitOfWork.SaveChangeAsync();

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Learning Registration updated successfully."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to update Learning Registration. " + ex.Message
                };
            }
        }

        public async Task<ResponseDTO> JoinClassWithWalletPaymentAsync(LearnerClassPaymentDTO paymentDTO)
        {
            try
            {
                _logger.LogInformation($"Starting class enrollment process for learner ID: {paymentDTO.LearnerId}, class ID: {paymentDTO.ClassId}");

                // Start transaction
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                // 1. Verify the class exists
                var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(paymentDTO.ClassId);
                if (classEntity == null)
                {
                    _logger.LogWarning($"Class with ID {paymentDTO.ClassId} not found");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Class with ID {paymentDTO.ClassId} not found."
                    };
                }

                // 2. Verify the learner exists
                var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(paymentDTO.LearnerId);
                if (learner == null)
                {
                    _logger.LogWarning($"Learner with ID {paymentDTO.LearnerId} not found");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Learner with ID {paymentDTO.LearnerId} not found."
                    };
                }

                // 3. Check if learner is already enrolled in this class
                var existingEnrollment = await _unitOfWork.dbContext.Learner_Classes
                    .FirstOrDefaultAsync(lc => lc.LearnerId == paymentDTO.LearnerId && lc.ClassId == paymentDTO.ClassId);

                if (existingEnrollment != null)
                {
                    _logger.LogWarning($"Learner {paymentDTO.LearnerId} is already enrolled in class {paymentDTO.ClassId}");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "You are already enrolled in this class."
                    };
                }

                // 4. Get class price
                decimal classPrice = classEntity.Price;
                if (classPrice <= 0)
                {
                    _logger.LogWarning($"Invalid price for class {paymentDTO.ClassId}: {classPrice}");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Invalid class price."
                    };
                }

                // 5. Get Registration Type for Class
                var classRegisType = await _unitOfWork.LearningRegisTypeRepository.GetQuery()
                    .FirstOrDefaultAsync(rt => rt.RegisTypeName.Contains("Class"));

                if (classRegisType == null)
                {
                    _logger.LogWarning("Class registration type not found in the database");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Class registration type not found in the system."
                    };
                }

                // 6. Check wallet balance
                var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == paymentDTO.LearnerId);
                if (wallet == null)
                {
                    _logger.LogWarning($"Wallet not found for learner {paymentDTO.LearnerId}");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Wallet not found for your account."
                    };
                }

                if (wallet.Balance < classPrice)
                {
                    _logger.LogWarning($"Insufficient balance for learner {paymentDTO.LearnerId}. Required: {classPrice}, Available: {wallet.Balance}");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Insufficient balance. Required: {classPrice}, Available: {wallet.Balance}"
                    };
                }

                // 7. Deduct payment from wallet
                wallet.Balance -= classPrice;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);
                await _unitOfWork.SaveChangeAsync();

                // 8. Create wallet transaction record
                var walletTransaction = new WalletTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    WalletId = wallet.WalletId,
                    Amount = classPrice,
                    TransactionType = TransactionType.Payment,
                    Status = Model.Enum.TransactionStatus.Complete,
                    //Description = $"Payment for Class: {classEntity.ClassName}",
                    TransactionDate = DateTime.UtcNow
                };

                await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);
                await _unitOfWork.SaveChangeAsync();

                // 9. Create Learner_class entry
                var learnerClass = new Learner_class
                {
                    LearnerId = paymentDTO.LearnerId,
                    ClassId = paymentDTO.ClassId,
                    //JoinDate = DateTime.UtcNow
                };

                await _unitOfWork.dbContext.Learner_Classes.AddAsync(learnerClass);
                await _unitOfWork.SaveChangeAsync();

                // 10. Create learning registration for tracking
                var learningRegis = new Learning_Registration
                {
                    LearnerId = paymentDTO.LearnerId,
                    ClassId = paymentDTO.ClassId,
                    TeacherId = classEntity.TeacherId,
                    RegisTypeId = classRegisType.RegisTypeId,
                    //MajorId = classEntity.CoursePackage.MajorId, // Assuming CoursePackage has MajorId
                    Status = LearningRegis.Accepted,
                    RequestDate = DateTime.UtcNow,
                    Price = classPrice,
                    NumberOfSession = classEntity.totalDays, // Using class total days as number of sessions
                    TimeStart = classEntity.ClassTime, // Using class time
                    TimeLearning = 120 // Default 2 hours (can adjust based on your class duration)
                };

                await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);
                await _unitOfWork.SaveChangeAsync();

                // 11. Create class schedules for this learner based on class schedule
                var classSchedules = await _unitOfWork.ScheduleRepository.GetClassSchedulesByTeacherIdAsync(classEntity.TeacherId);

                if (classSchedules != null && classSchedules.Any())
                {
                    var filteredSchedules = classSchedules.Where(s => s.ClassId == paymentDTO.ClassId).ToList();
                    if (filteredSchedules.Any())
                    {
                        var learnerSchedules = new List<Schedules>();

                        foreach (var schedule in filteredSchedules)
                        {
                            var learnerSchedule = new Schedules
                            {
                                LearnerId = paymentDTO.LearnerId,
                                ClassId = paymentDTO.ClassId,
                                LearningRegisId = learningRegis.LearningRegisId,
                                TeacherId = schedule.TeacherId,
                                StartDay = schedule.StartDay,
                                TimeStart = schedule.TimeStart,
                                TimeEnd = schedule.TimeEnd,
                                Mode = ScheduleMode.Center
                            };

                            learnerSchedules.Add(learnerSchedule);
                        }

                        await _unitOfWork.ScheduleRepository.AddRangeAsync(learnerSchedules);
                        await _unitOfWork.SaveChangeAsync();
                    }
                    else
                    {
                        // If no schedules found, create them based on class days
                        var classDays = await _unitOfWork.ClassDayRepository.GetQuery()
                            .Where(cd => cd.ClassId == paymentDTO.ClassId)
                            .ToListAsync();

                        if (classDays.Any())
                        {
                            var learnerSchedules = new List<Schedules>();
                            var startDay = classEntity.StartDate;

                            foreach (var classDay in classDays)
                            {
                                // Find the next occurrence of this day of week starting from the class start date
                                var scheduleDay = GetNextDayOfWeek(startDay, classDay.Day);

                                var learnerSchedule = new Schedules
                                {
                                    LearnerId = paymentDTO.LearnerId,
                                    ClassId = paymentDTO.ClassId,
                                    LearningRegisId = learningRegis.LearningRegisId,
                                    TeacherId = classEntity.TeacherId,
                                    StartDay = scheduleDay,
                                    TimeStart = classEntity.ClassTime,
                                    TimeEnd = classEntity.ClassTime.AddHours(2), // Assuming 2-hour classes
                                    Mode = ScheduleMode.Center
                                };

                                learnerSchedules.Add(learnerSchedule);
                            }

                            await _unitOfWork.ScheduleRepository.AddRangeAsync(learnerSchedules);
                            await _unitOfWork.SaveChangeAsync();
                        }
                        else
                        {
                            _logger.LogWarning($"No class days found for class {paymentDTO.ClassId}");
                        }
                    }
                }

                // 12. Create a test result record for the learner in this class
                var testResult = new Test_Result
                {
                    LearnerId = paymentDTO.LearnerId,
                    TeacherId = classEntity.TeacherId,
                    //MajorId = classEntity.CoursePackage.MajorId, // Assuming CoursePackage has MajorId
                    LearningRegisId = learningRegis.LearningRegisId,
                    ResultType = TestResultType.Center,
                    Status = TestResultStatus.Pending
                };

                await _unitOfWork.TestResultRepository.AddAsync(testResult);
                await _unitOfWork.SaveChangeAsync();

                // Commit the transaction
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Learner {paymentDTO.LearnerId} successfully enrolled in class {paymentDTO.ClassId} with payment of {classPrice}");

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"You have successfully enrolled in the class '{classEntity.ClassName}'. Payment of {classPrice} has been processed.",
                    Data = new
                    {
                        LearningRegisId = learningRegis.LearningRegisId,
                        LearnerId = paymentDTO.LearnerId,
                        ClassId = paymentDTO.ClassId,
                        AmountPaid = classPrice,
                        TransactionId = walletTransaction.TransactionId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing class enrollment with payment");
                await _unitOfWork.RollbackTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to enroll in class: {ex.Message}"
                };
            }
        }

        // Helper method to get the next occurrence of a specific day of week
        private DateOnly GetNextDayOfWeek(DateOnly startDate, DayOfWeeks dayOfWeek)
        {
            int daysToAdd = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;
            // If today is the day we want but we want to find the next occurrence, add 7 days
            if (daysToAdd == 0)
                daysToAdd = 7;

            return startDate.AddDays(daysToAdd);
        }
    }
}
