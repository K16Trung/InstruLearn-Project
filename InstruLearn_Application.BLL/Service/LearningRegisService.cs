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
        private readonly IScheduleService _scheduleService;

        public LearningRegisService(ILearningRegisRepository learningRegisRepository, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LearningRegisService> logger, IScheduleService scheduleService)
        {
            _learningRegisRepository = learningRegisRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _scheduleService = scheduleService;
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

                var scheduleConflict = await _scheduleService.CheckLearnerScheduleConflictAsync(createLearningRegisDTO.LearnerId, createLearningRegisDTO.StartDay.Value, createLearningRegisDTO.TimeStart, createLearningRegisDTO.TimeLearning);

                if (!scheduleConflict.IsSucceed)
                {
                    return scheduleConflict; // Return the conflict response
                }

                // Check for duplicate registrations - prevent spam registrations
                var existingRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(r =>
                        r.LearnerId == createLearningRegisDTO.LearnerId &&
                        //r.MajorId == createLearningRegisDTO.MajorId && 
                        r.TimeStart == createLearningRegisDTO.TimeStart &&
                        //r.TimeLearning == createLearningRegisDTO.TimeLearning &&
                        r.Status == LearningRegis.Pending)
                    .ToListAsync();

                if (existingRegistrations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "You already have a pending registration for this major. Please wait for it to be processed before creating a new one."
                    };
                }

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
                learningRegis.AcceptedDate = DateTime.Now;
                learningRegis.PaymentDeadline = DateTime.Now.AddDays(3);

                learningRegis.LearningPath = levelAssigned.SyllabusLink;

                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                await _unitOfWork.SaveChangeAsync();

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Calculate total price and payment amount (40%)
                decimal totalPrice = learningRegis.Price.Value;
                decimal paymentAmount = totalPrice * 0.4m;

                var deadline = learningRegis.PaymentDeadline.Value.ToString("yyyy-MM-dd HH:mm");

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Learning Registration updated successfully. Payment of {paymentAmount:F2} VND (40% of total {totalPrice:F2} VND) is required by {deadline}.",
                    Data = new
                    {
                        LearningRegisId = learningRegis.LearningRegisId,
                        PaymentAmount = paymentAmount,
                        TotalPrice = totalPrice,
                        PaymentDeadline = deadline,
                        SyllabusLink = levelAssigned.SyllabusLink
                    }
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
                var classScheduleConflict = await _scheduleService.CheckLearnerClassScheduleConflictAsync(paymentDTO.LearnerId, paymentDTO.ClassId);

                if (!classScheduleConflict.IsSucceed)
                {
                    return classScheduleConflict; // Return the conflict response
                }

                // Check if learner is already enrolled or has a pending enrollment for this class
                var existingEnrollments = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .AnyAsync(lr =>
                        lr.LearnerId == paymentDTO.LearnerId &&
                        lr.ClassId == paymentDTO.ClassId &&
                        (lr.Status == LearningRegis.Pending || lr.Status == LearningRegis.Accepted || lr.Status == LearningRegis.Fourty || lr.Status == LearningRegis.Sixty));

                if (existingEnrollments)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "You already have an enrollment or pending enrollment for this class."
                    };
                }

                // Start transaction
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
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

                    // 4. Calculate class price - UPDATED PRICING LOGIC
                    // Per-day price from class entity
                    decimal pricePerDay = classEntity.Price;
                    if (pricePerDay <= 0)
                    {
                        _logger.LogWarning($"Invalid price for class {paymentDTO.ClassId}: {pricePerDay}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Invalid class price."
                        };
                    }

                    // Calculate total price based on total days
                    decimal totalClassPrice = pricePerDay * classEntity.totalDays;

                    // Calculate 10% payment amount required
                    decimal paymentAmount = totalClassPrice * 0.1m;

                    // Round to ensure clean numbers
                    paymentAmount = Math.Round(paymentAmount, 2);

                    _logger.LogInformation($"Class price calculation: {pricePerDay} per day × {classEntity.totalDays} days = {totalClassPrice} total. 10% payment: {paymentAmount}");

                    // 5. Get Registration Type for Class
                    var classRegisType = await _unitOfWork.LearningRegisTypeRepository.GetQuery()
                        .FirstOrDefaultAsync(rt => rt.RegisTypeName.Contains("Center"));

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

                    if (wallet.Balance < paymentAmount)
                    {
                        _logger.LogWarning($"Insufficient balance for learner {paymentDTO.LearnerId}. Required: {paymentAmount}, Available: {wallet.Balance}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Insufficient balance. Required: {paymentAmount} (10% of total {totalClassPrice}), Available: {wallet.Balance}"
                        };
                    }

                    // 7. Deduct payment from wallet
                    wallet.Balance -= paymentAmount;
                    var walletUpdateResult = await _unitOfWork.WalletRepository.UpdateAsync(wallet);
                    if (!walletUpdateResult)
                    {
                        throw new Exception("Failed to update wallet balance");
                    }
                    await _unitOfWork.SaveChangeAsync();

                    // 8. Create wallet transaction record
                    var walletTransaction = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        WalletId = wallet.WalletId,
                        Amount = paymentAmount,
                        TransactionType = TransactionType.Payment,
                        Status = Model.Enum.TransactionStatus.Complete,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);
                    await _unitOfWork.SaveChangeAsync();

                    // 9. Create learning registration for tracking
                    var learningRegis = new Learning_Registration
                    {
                        LearnerId = paymentDTO.LearnerId,
                        ClassId = paymentDTO.ClassId,
                        TeacherId = classEntity.TeacherId,
                        RegisTypeId = classRegisType.RegisTypeId,
                        MajorId = classEntity.MajorId,
                        Status = LearningRegis.Accepted,
                        RequestDate = DateTime.UtcNow,
                        Price = totalClassPrice, // Store the TOTAL price in learning registration
                        NumberOfSession = classEntity.totalDays,
                        TimeStart = classEntity.ClassTime,
                        TimeLearning = 120, // Default 2 hours
                        StartDay = classEntity.StartDate,
                        VideoUrl = string.Empty,
                        LearningRequest = string.Empty
                    };

                    await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    // 10. Create Learner_class entry
                    var learnerClass = new Learner_class
                    {
                        LearnerId = paymentDTO.LearnerId,
                        ClassId = paymentDTO.ClassId
                    };

                    await _unitOfWork.dbContext.Learner_Classes.AddAsync(learnerClass);
                    await _unitOfWork.SaveChangeAsync();

                    // 11. Create a test result record for the learner in this class
                    var testResult = new Test_Result
                    {
                        LearnerId = paymentDTO.LearnerId,
                        TeacherId = classEntity.TeacherId,
                        MajorId = classEntity.MajorId,
                        LearningRegisId = learningRegis.LearningRegisId,
                        ResultType = TestResultType.Center,
                        Status = TestResultStatus.Pending
                    };

                    await _unitOfWork.TestResultRepository.AddAsync(testResult);
                    await _unitOfWork.SaveChangeAsync();

                    // 12. REVISED APPROACH: Find existing schedules for other learners in this class to use as a template
                    // This ensures that all learners get the exact same schedule pattern
                    var existingLearnerSchedules = await _unitOfWork.ScheduleRepository.GetQuery()
                        .Where(s => s.ClassId == paymentDTO.ClassId &&
                                   s.TeacherId == classEntity.TeacherId &&
                                   s.LearnerId != null)
                        .OrderBy(s => s.StartDay)
                        .ToListAsync();

                    if (existingLearnerSchedules != null && existingLearnerSchedules.Any())
                    {
                        _logger.LogInformation($"Found {existingLearnerSchedules.Count} existing schedules for other learners in class {paymentDTO.ClassId}");

                        // Group schedules by start day to get unique dates
                        var uniqueDates = existingLearnerSchedules
                            .GroupBy(s => s.StartDay)
                            .Select(g => g.First())
                            .OrderBy(s => s.StartDay)
                            .Take(classEntity.totalDays) // Ensure we only take what we need
                            .ToList();

                        // Create new schedules for this learner using the same pattern as existing learners
                        var newSchedules = new List<Schedules>();

                        foreach (var existingSchedule in uniqueDates)
                        {
                            var newSchedule = new Schedules
                            {
                                LearnerId = paymentDTO.LearnerId,
                                ClassId = paymentDTO.ClassId,
                                LearningRegisId = learningRegis.LearningRegisId,
                                TeacherId = classEntity.TeacherId,
                                StartDay = existingSchedule.StartDay, // Use the exact same day pattern
                                TimeStart = classEntity.ClassTime,
                                TimeEnd = classEntity.ClassTime.AddHours(2),
                                Mode = ScheduleMode.Center
                            };

                            newSchedules.Add(newSchedule);
                        }

                        await _unitOfWork.ScheduleRepository.AddRangeAsync(newSchedules);
                        await _unitOfWork.SaveChangeAsync();
                    }
                    else
                    {
                        // No existing schedules with learners found, try to use teacher schedules or create new ones
                        var existingTeacherSchedules = await _unitOfWork.ScheduleRepository.GetQuery()
                            .Where(s => s.ClassId == paymentDTO.ClassId &&
                                       s.TeacherId == classEntity.TeacherId &&
                                       s.LearnerId == null)
                            .OrderBy(s => s.StartDay)
                            .ToListAsync();

                        if (existingTeacherSchedules != null && existingTeacherSchedules.Any())
                        {
                            _logger.LogInformation($"Found {existingTeacherSchedules.Count} existing teacher schedules for class {paymentDTO.ClassId}");

                            // Keep track of how many schedules we've assigned
                            int schedulesUsed = 0;
                            var learnerSchedules = new List<Schedules>();

                            foreach (var teacherSchedule in existingTeacherSchedules.OrderBy(s => s.StartDay))
                            {
                                if (schedulesUsed >= classEntity.totalDays)
                                    break;

                                var learnerSchedule = new Schedules
                                {
                                    LearnerId = paymentDTO.LearnerId,
                                    ClassId = paymentDTO.ClassId,
                                    LearningRegisId = learningRegis.LearningRegisId,
                                    TeacherId = classEntity.TeacherId,
                                    StartDay = teacherSchedule.StartDay,
                                    TimeStart = classEntity.ClassTime,
                                    TimeEnd = classEntity.ClassTime.AddHours(2),
                                    Mode = ScheduleMode.Center
                                };

                                learnerSchedules.Add(learnerSchedule);
                                schedulesUsed++;
                            }

                            await _unitOfWork.ScheduleRepository.AddRangeAsync(learnerSchedules);
                            await _unitOfWork.SaveChangeAsync();
                        }
                        else
                        {
                            _logger.LogWarning($"No existing schedules found for class {paymentDTO.ClassId}, creating new ones");

                            // Create completely new schedules if no existing schedules found
                            var classDays = await _unitOfWork.ClassDayRepository.GetQuery()
                                .Where(cd => cd.ClassId == paymentDTO.ClassId)
                                .OrderBy(cd => cd.Day) // Ensure consistent order
                                .ToListAsync();

                            if (classDays.Any())
                            {
                                var learnerSchedules = new List<Schedules>();
                                var startDay = classEntity.StartDate;
                                int schedulesCreated = 0;
                                int weekMultiplier = 0;

                                // Pre-calculate all the schedule days for consistency
                                var scheduleDays = new List<DateOnly>();

                                while (scheduleDays.Count < classEntity.totalDays)
                                {
                                    foreach (var classDay in classDays.OrderBy(cd => cd.Day))
                                    {
                                        if (scheduleDays.Count >= classEntity.totalDays)
                                            break;

                                        var scheduleDay = GetDateForDayOfWeek(startDay, classDay.Day, weekMultiplier);
                                        scheduleDays.Add(scheduleDay);
                                    }

                                    weekMultiplier++;
                                }

                                // Create schedules in order of dates
                                foreach (var scheduleDay in scheduleDays.OrderBy(d => d))
                                {
                                    var learnerSchedule = new Schedules
                                    {
                                        LearnerId = paymentDTO.LearnerId,
                                        ClassId = paymentDTO.ClassId,
                                        LearningRegisId = learningRegis.LearningRegisId,
                                        TeacherId = classEntity.TeacherId,
                                        StartDay = scheduleDay,
                                        TimeStart = classEntity.ClassTime,
                                        TimeEnd = classEntity.ClassTime.AddHours(2),
                                        Mode = ScheduleMode.Center
                                    };

                                    learnerSchedules.Add(learnerSchedule);
                                    schedulesCreated++;

                                    if (schedulesCreated >= classEntity.totalDays)
                                        break;
                                }

                                await _unitOfWork.ScheduleRepository.AddRangeAsync(learnerSchedules);
                                await _unitOfWork.SaveChangeAsync();
                            }
                            else
                            {
                                _logger.LogWarning($"No class days found for class {paymentDTO.ClassId}. Enrollment may be incomplete.");
                            }
                        }
                    }

                    // Commit the transaction
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation($"Learner {paymentDTO.LearnerId} successfully enrolled in class {paymentDTO.ClassId} with payment of {paymentAmount} (10% of total {totalClassPrice})");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"You have successfully enrolled in the class '{classEntity.ClassName}'. Payment of {paymentAmount} (10% of total {totalClassPrice}) has been processed.",
                        Data = new
                        {
                            LearningRegisId = learningRegis.LearningRegisId,
                            LearnerId = paymentDTO.LearnerId,
                            ClassId = paymentDTO.ClassId,
                            AmountPaid = paymentAmount,
                            TotalClassPrice = totalClassPrice
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Detailed error during class enrollment: {Message}", ex.Message);
                    if (ex.InnerException != null)
                    {
                        _logger.LogError("Inner exception: {Message}", ex.InnerException.Message);
                    }

                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing class enrollment with payment");

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to enroll in class: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseDTO> RejectLearningRegisAsync(int learningRegisId, string rejectReason)
        {
            try
            {
                _logger.LogInformation($"Starting learning registration rejection process for registration ID: {learningRegisId}");

                // Find the registration
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
                if (learningRegis == null)
                {
                    _logger.LogWarning($"Learning Registration with ID {learningRegisId} not found");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found."
                    };
                }

                // Verify it's in a "Pending" state - only pending registrations can be rejected
                if (learningRegis.Status != LearningRegis.Pending)
                {
                    _logger.LogWarning($"Cannot reject registration {learningRegisId} with status {learningRegis.Status}. Only pending registrations can be rejected.");
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Cannot reject registration with status {learningRegis.Status}. Only pending registrations can be rejected."
                    };
                }

                // Start a transaction to ensure atomic operations
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // Update the registration status to rejected
                    learningRegis.Status = LearningRegis.Rejected;

                    // Add reason for rejection if provided
                    if (!string.IsNullOrEmpty(rejectReason))
                    {
                        learningRegis.LearningRequest = rejectReason; // Storing rejection reason in the LearningRequest field
                    }

                    await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    // Update associated test result if it exists
                    var testResult = await _unitOfWork.TestResultRepository.GetByLearningRegisIdAsync(learningRegisId);
                    if (testResult != null)
                    {
                        testResult.Status = TestResultStatus.Cancelled;
                        await _unitOfWork.TestResultRepository.UpdateAsync(testResult);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    // Commit the transaction
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation($"Learning registration {learningRegisId} successfully rejected without refund");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Learning Registration rejected successfully. No refund has been processed.",
                        Data = new
                        {
                            LearningRegisId = learningRegisId,
                            LearnerId = learningRegis.LearnerId,
                            Status = "Rejected"
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during rejection of learning registration {learningRegisId}: {ex.Message}");
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing learning registration rejection: {ex.Message}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to reject learning registration: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateLearningPathSessionsAsync(LearningPathSessionsCreateDTO createDTO)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(createDTO.LearningRegisId);
                if (learningRegis == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found."
                    };
                }

                // Check if number of sessions is valid
                if (createDTO.LearningPathSessions.Count > learningRegis.NumberOfSession)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Number of learning path sessions ({createDTO.LearningPathSessions.Count}) exceeds the number of sessions ({learningRegis.NumberOfSession})."
                    };
                }

                // First, check for duplicate session numbers in the payload
                var distinctSessionCount = createDTO.LearningPathSessions
                    .Select(s => s.SessionNumber)
                    .Distinct()
                    .Count();

                if (distinctSessionCount != createDTO.LearningPathSessions.Count)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Duplicate session numbers found in the request."
                    };
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                // Get existing learning path sessions for this registration
                var existingSessions = await _unitOfWork.LearningPathSessionRepository
                    .GetByLearningRegisIdAsync(createDTO.LearningRegisId);

                // Delete existing sessions if we're doing a complete replacement
                if (existingSessions.Any())
                {
                    foreach (var session in existingSessions)
                    {
                        await _unitOfWork.LearningPathSessionRepository
                            .DeleteAsync(session.LearningPathSessionId);
                    }
                    await _unitOfWork.SaveChangeAsync();
                }

                // Create new learning path sessions
                var learningPathSessions = createDTO.LearningPathSessions.Select(lps => new LearningPathSession
                {
                    LearningRegisId = learningRegis.LearningRegisId,
                    SessionNumber = lps.SessionNumber,
                    Title = lps.Title,
                    Description = lps.Description,
                    IsCompleted = lps.IsCompleted, // Use the value from the DTO
                    CreatedAt = DateTime.Now
                }).ToList();

                await _unitOfWork.LearningPathSessionRepository.AddRangeAsync(learningPathSessions);
                await _unitOfWork.SaveChangeAsync();

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Learning path sessions created successfully for Learning Registration {createDTO.LearningRegisId}.",
                    Data = createDTO.LearningPathSessions.Count
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to create Learning Path Sessions. " + ex.Message
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

        private DateOnly GetDateForDayOfWeek(DateOnly startDay, DayOfWeeks targetDay, int weekMultiplier = 0)
        {
            // Find the first occurrence of the target day on or after the start date
            int daysToAdd = ((int)targetDay - (int)startDay.DayOfWeek + 7) % 7;

            // Add weeks based on the multiplier
            daysToAdd += (weekMultiplier * 7);

            return startDay.AddDays(daysToAdd);
        }

    }
}
