using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using InstruLearn_Application.Model.Models.DTO.Schedules;
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
                    .GetWithIncludesAsync(x => x.LearningRegisId == learningRegisId, "Teacher,Learner,LearningRegistrationDay,Schedules");

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
                    .SkipWhile(day => (DayOfWeek)day.DayOfWeek < currentDayOfWeek) // Skip days before the registration day
                    .Concat(orderedLearningDays) // Handle circular day-of-week loop
                    .Take(orderedLearningDays.Count); // Avoid duplicate iterations

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
                                StartDate = nextSessionDate.ToString("yyyy-MM-dd"),
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


        public async Task<ResponseDTO> GetSchedulesByLearnerIdAsync(int learnerId)
        {
            try
            {
                var learningRegisList = await _unitOfWork.LearningRegisRepository
                    .GetAllWithIncludesAsync(x => x.LearnerId == learnerId, "Teacher,Learner,LearningRegistrationDay,Schedules");

                if (learningRegisList == null || !learningRegisList.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No learning registrations found for this learner."
                    };
                }

                var allSchedules = GenerateSchedulesForRegistrations(learningRegisList);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Schedules retrieved successfully.",
                    Data = allSchedules
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

        public async Task<ResponseDTO> GetSchedulesByTeacherIdAsync(int teacherId)
        {
            try
            {
                var learningRegisList = await _unitOfWork.LearningRegisRepository
                    .GetAllWithIncludesAsync(x => x.TeacherId == teacherId, "Teacher,Learner,LearningRegistrationDay,Schedules");

                if (learningRegisList == null || !learningRegisList.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No learning registrations found for this teacher."
                    };
                }

                var allSchedules = GenerateSchedulesForRegistrations(learningRegisList);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Schedules retrieved successfully.",
                    Data = allSchedules
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

        private List<ScheduleDTO> GenerateSchedulesForRegistrations(List<Learning_Registration> learningRegisList)
        {
            var schedules = new List<ScheduleDTO>();

            foreach (var learningRegis in learningRegisList)
            {
                int sessionCount = 0;
                var startDate = learningRegis.StartDay.Value.ToDateTime(TimeOnly.MinValue);
                var registrationStartDate = startDate;
                var orderedLearningDays = learningRegis.LearningRegistrationDay.OrderBy(day => day.DayOfWeek).ToList();

                var currentDayOfWeek = registrationStartDate.DayOfWeek;

                var learningDaysStartingFromRegistration = orderedLearningDays
                    .SkipWhile(day => (DayOfWeek)day.DayOfWeek < currentDayOfWeek)
                    .Concat(orderedLearningDays)
                    .Take(orderedLearningDays.Count);

                while (sessionCount < learningRegis.NumberOfSession)
                {
                    foreach (var learningDay in learningDaysStartingFromRegistration)
                    {
                        DateTime nextSessionDate = registrationStartDate;
                        while (nextSessionDate.DayOfWeek != (DayOfWeek)learningDay.DayOfWeek)
                        {
                            nextSessionDate = nextSessionDate.AddDays(1);  // Move to the correct day of the week
                        }

                        if (sessionCount < learningRegis.NumberOfSession)
                        {
                            var existingSchedule = learningRegis.Schedules.ElementAtOrDefault(sessionCount);

                            var schedule = new ScheduleDTO
                            {
                                ScheduleId = existingSchedule?.ScheduleId ?? 0,
                                Mode = existingSchedule?.Mode ?? 0,
                                TimeStart = learningRegis.TimeStart.ToString("HH:mm"),
                                TimeEnd = learningRegis.TimeStart.AddMinutes(learningRegis.TimeLearning).ToString("HH:mm"),
                                DayOfWeek = nextSessionDate.DayOfWeek.ToString(),
                                StartDate = nextSessionDate.ToString("yyyy-MM-dd"),
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
                            sessionCount++;
                        }

                        registrationStartDate = nextSessionDate.AddDays(1);
                    }
                }
            }

            return schedules;
        }
    }
}
