using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ScheduleDTO>> GetSchedulesByLearningRegisIdAsync(int learningRegisId)
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetSchedulesByLearningRegisIdAsync(learningRegisId);
            return _mapper.Map<List<ScheduleDTO>>(schedules);
        }

        public async Task<ResponseDTO> GetSchedulesAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _unitOfWork.LearningRegisRepository
                    .GettWithIncludesAsync(x => x.LearningRegisId == learningRegisId, "Teacher,Learner,LearningRegistrationDay,Schedules");

                if (learningRegis == null || !learningRegis.StartDay.HasValue)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning Registration not found or Start Day is missing."
                    };
                }

                var startDate = learningRegis.StartDay.Value.ToDateTime(TimeOnly.MinValue);
                var registrationStartDate = startDate;  // Start from RegistrationStartDay
                var schedules = new List<ScheduleDTO>();
                int sessionCount = 0;

                // Get the ordered learning days and start from the registration day's day of the week
                var orderedLearningDays = learningRegis.LearningRegistrationDay.OrderBy(day => day.DayOfWeek).ToList();

                // Find the first valid session day from the registrationStartDay's day of the week
                var currentDayOfWeek = registrationStartDate.DayOfWeek;
                var learningDaysStartingFromRegistration = orderedLearningDays
                    .Where(day => (DayOfWeek)day.DayOfWeek >= currentDayOfWeek) // Start from the first available learning day
                    .Concat(orderedLearningDays.Where(day => (DayOfWeek)day.DayOfWeek < currentDayOfWeek)) // Append remaining days
                    .ToList();

                // Loop until the required number of sessions is generated
                while (sessionCount < learningRegis.NumberOfSession)
                {
                    foreach (var learningDay in learningDaysStartingFromRegistration)
                    {
                        // Calculate the next session date matching the learning day of the week
                        var nextSessionDate = GetNextSessionDate(registrationStartDate, (DayOfWeek)learningDay.DayOfWeek);

                        // Proceed only if the session count is within limit
                        if (sessionCount < learningRegis.NumberOfSession)
                        {
                            var existingSchedule = learningRegis.Schedules.ElementAtOrDefault(sessionCount);

                            // Create a new schedule DTO
                            var schedule = new ScheduleDTO
                            {
                                ScheduleId = existingSchedule?.ScheduleId ?? 0,
                                Mode = existingSchedule?.Mode ?? 0,
                                TimeStart = learningRegis.TimeStart.ToString("HH:mm"),
                                TimeEnd = learningRegis.TimeStart.AddMinutes(learningRegis.TimeLearning).ToString("HH:mm"),
                                DayOfWeek = nextSessionDate.DayOfWeek.ToString(),
                                //StartDay = nextSessionDate.ToString("yyyy-MM-dd"),
                                TeacherId = learningRegis.TeacherId,
                                LearnerId = learningRegis.LearnerId,
                                RegistrationStartDay = learningRegis.StartDay,
                                LearningRegisId = learningRegis.LearningRegisId,
                                ScheduleDays = orderedLearningDays.Select(day => new ScheduleDaysDTO
                                {
                                    DayOfWeeks = (DayOfWeeks)day.DayOfWeek
                                }).ToList()
                            };

                            schedules.Add(schedule);
                            sessionCount++;  // Increment session count after adding a schedule
                        }

                        // Move to the next day for the following iteration
                        registrationStartDate = nextSessionDate.AddDays(1);
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Schedules retrieved successfully.",
                    Data = schedules
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to retrieve schedules. " + ex.Message
                };
            }
        }

        // Helper method to calculate the next session date based on a start date and day of the week
        private DateTime GetNextSessionDate(DateTime startDate, DayOfWeek dayOfWeek)
        {
            int daysUntilNextSession = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;
            return startDate.AddDays(daysUntilNextSession);
        }

        // get correct
        public async Task<ResponseDTO> GetSchedulesByLearnerIdAsync(int learnerId)
        {
            try
            {
                if (learnerId <= 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Invalid learner ID."
                    };
                }

                // Fetch learning registrations for the learner with all necessary includes
                var learningRegs = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.LearnerId == learnerId && x.Status == LearningRegis.Completed,
                        "Teacher,Learner,LearningRegistrationDay,Schedules"
                    );

                if (learningRegs == null || !learningRegs.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No Learning Registrations found for this learner."
                    };
                }

                var schedules = new List<ScheduleDTO>();

                foreach (var learningRegis in learningRegs)
                {
                    if (!learningRegis.StartDay.HasValue)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Start day is missing."
                        };
                    }

                    // Use DateOnly instead of DateTime for the start date
                    DateOnly registrationStartDate = learningRegis.StartDay.Value;
                    var orderedLearningDays = learningRegis.LearningRegistrationDay
                        .OrderBy(day => day.DayOfWeek)
                        .ToList();

                    var existingSchedules = learningRegis.Schedules
                        .Where(s => s.Mode == ScheduleMode.OneOnOne)
                        .OrderBy(s => s.StartDay)
                        .ToList();

                    int sessionCount = 0;
                    int existingScheduleCount = existingSchedules.Count;
                    DateOnly currentDate = registrationStartDate;

                    // Generate schedules based on the number of sessions
                    while (sessionCount < learningRegis.NumberOfSession)
                    {
                        // Find the next valid learning day from the current date
                        DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
                        var nextLearningDays = orderedLearningDays
                            .OrderBy(ld => ((int)ld.DayOfWeek - (int)currentDayOfWeek + 7) % 7)
                            .ToList();

                        if (!nextLearningDays.Any())
                            break;

                        // Get the next closest learning day
                        var nextLearningDay = nextLearningDays.First();
                        int daysToAdd = ((int)nextLearningDay.DayOfWeek - (int)currentDayOfWeek + 7) % 7;

                        // If we are already on a learning day (daysToAdd would be 0), use that day
                        // Otherwise, advance to the next scheduled day
                        DateOnly scheduleStartDate = currentDate.AddDays(daysToAdd);

                        // Get existing schedule if one exists for this session
                        var existingSchedule = sessionCount < existingScheduleCount ? existingSchedules[sessionCount] : null;

                        // Create schedule if it doesn't already exist in our list for this date and learning registration
                        if (!schedules.Any(s => s.StartDay == scheduleStartDate && s.LearningRegisId == learningRegis.LearningRegisId))
                        {
                            // Ensure LearningRegisId is set in the schedule
                            schedules.Add(new ScheduleDTO
                            {
                                ScheduleId = existingSchedule?.ScheduleId ?? 0,
                                Mode = existingSchedule?.Mode ?? ScheduleMode.OneOnOne,
                                TimeStart = learningRegis.TimeStart.ToString("HH:mm"),
                                TimeEnd = learningRegis.TimeStart.AddMinutes(learningRegis.TimeLearning).ToString("HH:mm"),
                                DayOfWeek = scheduleStartDate.DayOfWeek.ToString(),
                                StartDay = scheduleStartDate, // Store the DateOnly object
                                TeacherId = learningRegis.TeacherId,
                                TeacherName = learningRegis.Teacher?.Fullname ?? "N/A",
                                LearnerId = learningRegis.LearnerId,
                                LearnerName = learningRegis.Learner?.FullName ?? "N/A",
                                ClassId = learningRegis.ClassId,
                                ClassName = learningRegis.Classes?.ClassName ?? "N/A",
                                RegistrationStartDay = learningRegis.StartDay,
                                LearningRegisId = learningRegis.LearningRegisId,
                                ScheduleDays = orderedLearningDays.Select(day => new ScheduleDaysDTO
                                {
                                    DayOfWeeks = (DayOfWeeks)day.DayOfWeek
                                }).ToList()
                            });

                            sessionCount++;
                        }

                        // Move to the next day to find the next schedule
                        currentDate = scheduleStartDate.AddDays(1);
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Schedules retrieved successfully.",
                    Data = schedules
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to retrieve schedules: {ex.Message}"
                };
            }
        }

        // get correct
        public async Task<ResponseDTO> GetSchedulesByTeacherIdAsync(int teacherId)
        {
            try
            {
                if (teacherId <= 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Invalid teacher ID."
                    };
                }

                // Fetch learning registrations for the teacher with all necessary includes
                var learningRegs = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.TeacherId == teacherId && x.Status == LearningRegis.Completed,
                        "Teacher,Learner,LearningRegistrationDay,Schedules"
                    );

                if (learningRegs == null || !learningRegs.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No Learning Registrations found for this teacher."
                    };
                }

                var schedules = new List<ScheduleDTO>();

                foreach (var learningRegis in learningRegs)
                {
                    if (!learningRegis.StartDay.HasValue)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Start day is missing."
                        };
                    }

                    // Use DateOnly instead of DateTime for the start date
                    DateOnly registrationStartDate = learningRegis.StartDay.Value;
                    var orderedLearningDays = learningRegis.LearningRegistrationDay
                        .OrderBy(day => day.DayOfWeek)
                        .ToList();

                    var existingSchedules = learningRegis.Schedules
                        .Where(s => s.Mode == ScheduleMode.OneOnOne)
                        .OrderBy(s => s.StartDay)
                        .ToList();

                    int sessionCount = 0;
                    int existingScheduleCount = existingSchedules.Count;
                    DateOnly currentDate = registrationStartDate;

                    // Generate schedules based on the number of sessions
                    while (sessionCount < learningRegis.NumberOfSession)
                    {
                        // Find the next valid learning day from the current date
                        DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
                        var nextLearningDays = orderedLearningDays
                            .OrderBy(ld => ((int)ld.DayOfWeek - (int)currentDayOfWeek + 7) % 7)
                            .ToList();

                        if (!nextLearningDays.Any())
                            break;

                        // Get the next closest learning day
                        var nextLearningDay = nextLearningDays.First();
                        int daysToAdd = ((int)nextLearningDay.DayOfWeek - (int)currentDayOfWeek + 7) % 7;

                        // If we are already on a learning day (daysToAdd would be 0), use that day
                        // Otherwise, advance to the next scheduled day
                        DateOnly scheduleStartDate = currentDate.AddDays(daysToAdd);

                        // Get existing schedule if one exists for this session
                        var existingSchedule = sessionCount < existingScheduleCount ? existingSchedules[sessionCount] : null;

                        // Create schedule if it doesn't already exist in our list for this date and learning registration
                        if (!schedules.Any(s => s.StartDay == scheduleStartDate && s.LearningRegisId == learningRegis.LearningRegisId))
                        {
                            // Ensure LearningRegisId is set in the schedule
                            schedules.Add(new ScheduleDTO
                            {
                                ScheduleId = existingSchedule?.ScheduleId ?? 0,
                                Mode = existingSchedule?.Mode ?? ScheduleMode.OneOnOne,
                                TimeStart = learningRegis.TimeStart.ToString("HH:mm"),
                                TimeEnd = learningRegis.TimeStart.AddMinutes(learningRegis.TimeLearning).ToString("HH:mm"),
                                DayOfWeek = scheduleStartDate.DayOfWeek.ToString(),
                                StartDay = scheduleStartDate, // Store the DateOnly object
                                TeacherId = learningRegis.TeacherId,
                                TeacherName = learningRegis.Teacher?.Fullname ?? "N/A",
                                LearnerId = learningRegis.LearnerId,
                                LearnerName = learningRegis.Learner?.FullName ?? "N/A",
                                LearnerAddress = learningRegis.Learner?.Account?.Address ?? "N/A", // Add this line to include the learner's address
                                RegistrationStartDay = learningRegis.StartDay,
                                LearningRegisId = learningRegis.LearningRegisId,
                                ScheduleDays = orderedLearningDays.Select(day => new ScheduleDaysDTO
                                {
                                    DayOfWeeks = (DayOfWeeks)day.DayOfWeek
                                }).ToList()
                            });

                            sessionCount++;
                        }

                        // Move to the next day to find the next schedule
                        currentDate = scheduleStartDate.AddDays(1);
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Schedules retrieved successfully.",
                    Data = schedules
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to retrieve schedules: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetClassSchedulesByTeacherIdAsync(int teacherId)
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetClassSchedulesByTeacherIdAsync(teacherId);

            if (schedules == null || !schedules.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy lịch học của giáo viên.",
                };
            }

            var scheduleDTOs = _mapper.Map<List<ScheduleDTO>>(schedules);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Lấy lịch học thành công.",
                Data = scheduleDTOs
            };
        }

        public async Task<ResponseDTO> GetClassSchedulesByTeacherIdAsyncc(int teacherId)
        {
            try
            {
                var consolidatedSchedules = await _unitOfWork.ScheduleRepository.GetConsolidatedCenterSchedulesByTeacherIdAsync(teacherId);

                if (consolidatedSchedules == null || !consolidatedSchedules.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy lịch học của giáo viên.",
                    };
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Lấy lịch học thành công.",
                    Data = consolidatedSchedules
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy lịch học: {ex.Message}"
                };
            }
        }


        public async Task<List<ValidTeacherDTO>> GetAvailableTeachersAsync(int majorId, TimeOnly timeStart, int timeLearning, DateOnly startDay)
        {
            var freeTeacherIds = await _unitOfWork.ScheduleRepository.GetFreeTeacherIdsAsync(majorId, timeStart, timeLearning, startDay);

            if (!freeTeacherIds.Any())
            {
                return new List<ValidTeacherDTO>(); // No available teachers
            }

            var availableTeachers = await _unitOfWork.TeacherRepository
                .GetSchedulesTeachersByIdsAsync(freeTeacherIds);

            return _mapper.Map<List<ValidTeacherDTO>>(availableTeachers);
        }
    }
}
