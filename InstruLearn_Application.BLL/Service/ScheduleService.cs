using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
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
                        x => x.LearnerId == learnerId && (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty),
                        "Teacher,Learner.Account,LearningRegistrationDay,Schedules"
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

                    // Get learning path sessions for this registration
                    var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                        .GetByLearningRegisIdAsync(learningRegis.LearningRegisId);

                    // Sort by session number
                    var sortedSessions = learningPathSessions
                        .OrderBy(s => s.SessionNumber)
                        .ToList();

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

                        DateOnly scheduleStartDate = currentDate.AddDays(daysToAdd);

                        var existingSchedule = sessionCount < existingScheduleCount ? existingSchedules[sessionCount] : null;

                        var sessionNumber = sessionCount + 1; // Session numbers typically start at 1
                        var learningPathSession = sortedSessions.FirstOrDefault(s => s.SessionNumber == sessionNumber);

                        // Create schedule if it doesn't already exist in our list for this date and learning registration
                        if (!schedules.Any(s => s.StartDay == scheduleStartDate && s.LearningRegisId == learningRegis.LearningRegisId))
                        {
                            // Ensure LearningRegisId is set in the schedule
                            var scheduleDto = new ScheduleDTO
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
                                LearnerAddress = learningRegis.Learner?.Account?.Address ?? "N/A",
                                ClassId = learningRegis.ClassId,
                                ClassName = learningRegis.Classes?.ClassName ?? "N/A",
                                RegistrationStartDay = learningRegis.StartDay,
                                LearningRegisId = learningRegis.LearningRegisId,
                                AttendanceStatus = existingSchedule?.AttendanceStatus ?? 0,
                                ScheduleDays = orderedLearningDays.Select(day => new ScheduleDaysDTO
                                {
                                    DayOfWeeks = (DayOfWeeks)day.DayOfWeek
                                }).ToList()
                            };

                            // Add learning path session information if available
                            if (learningPathSession != null)
                            {
                                scheduleDto.LearningPathSessionId = learningPathSession.LearningPathSessionId;
                                scheduleDto.SessionNumber = learningPathSession.SessionNumber;
                                scheduleDto.SessionTitle = learningPathSession.Title;
                                scheduleDto.SessionDescription = learningPathSession.Description;
                                scheduleDto.IsSessionCompleted = learningPathSession.IsCompleted;
                            }
                            else
                            {
                                // Default values if no session is found
                                scheduleDto.SessionNumber = sessionNumber;
                                scheduleDto.SessionTitle = $"Session {sessionNumber}";
                                scheduleDto.SessionDescription = "";
                                scheduleDto.IsSessionCompleted = false;
                            }

                            schedules.Add(scheduleDto);

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
                        x => x.TeacherId == teacherId && (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty),
                        "Teacher,Learner.Account,LearningRegistrationDay,Schedules"
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

                    // Get learning path sessions for this registration
                    var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                        .GetByLearningRegisIdAsync(learningRegis.LearningRegisId);

                    // Sort by session number
                    var sortedSessions = learningPathSessions
                        .OrderBy(s => s.SessionNumber)
                        .ToList();

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

                        var nextLearningDay = nextLearningDays.First();
                        int daysToAdd = ((int)nextLearningDay.DayOfWeek - (int)currentDayOfWeek + 7) % 7;

                        DateOnly scheduleStartDate = currentDate.AddDays(daysToAdd);

                        var existingSchedule = sessionCount < existingScheduleCount ? existingSchedules[sessionCount] : null;

                        var sessionNumber = sessionCount + 1;
                        var learningPathSession = sortedSessions.FirstOrDefault(s => s.SessionNumber == sessionNumber);

                        if (!schedules.Any(s => s.StartDay == scheduleStartDate && s.LearningRegisId == learningRegis.LearningRegisId))
                        {
                            var scheduleDto = new ScheduleDTO
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
                                LearnerAddress = learningRegis.Learner?.Account?.Address ?? "N/A",
                                ClassId = learningRegis.ClassId,
                                ClassName = learningRegis.Classes?.ClassName ?? "N/A",
                                RegistrationStartDay = learningRegis.StartDay,
                                LearningRegisId = learningRegis.LearningRegisId,
                                AttendanceStatus = existingSchedule?.AttendanceStatus ?? 0,
                                ScheduleDays = orderedLearningDays.Select(day => new ScheduleDaysDTO
                                {
                                    DayOfWeeks = (DayOfWeeks)day.DayOfWeek
                                }).ToList()
                            };

                            // Add learning path session information if available
                            if (learningPathSession != null)
                            {
                                scheduleDto.LearningPathSessionId = learningPathSession.LearningPathSessionId;
                                scheduleDto.SessionNumber = learningPathSession.SessionNumber;
                                scheduleDto.SessionTitle = learningPathSession.Title;
                                scheduleDto.SessionDescription = learningPathSession.Description;
                                scheduleDto.IsSessionCompleted = learningPathSession.IsCompleted;
                            }
                            else
                            {
                                // Default values if no session is found
                                scheduleDto.SessionNumber = sessionNumber;
                                scheduleDto.SessionTitle = $"Session {sessionNumber}";
                                scheduleDto.SessionDescription = "";
                                scheduleDto.IsSessionCompleted = false;
                            }

                            schedules.Add(scheduleDto);

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
                    Message = "Không tìm thấy lịch dạy của giáo viên.",
                };
            }

            var scheduleDTOs = _mapper.Map<List<ScheduleDTO>>(schedules);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Lấy lịch dạy thành công.",
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
                        Message = "Không tìm thấy lịch dạy của giáo viên.",
                    };
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Lấy lịch dạy thành công.",
                    Data = consolidatedSchedules
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi lấy lịch dạy: {ex.Message}"
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

        public async Task<ResponseDTO> GetClassSchedulesByLearnerIdAsync(int learnerId)
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

                // Fetch schedules for the learner
                var schedules = await _unitOfWork.ScheduleRepository.GetClassSchedulesByLearnerIdAsync(learnerId);

                if (schedules == null || !schedules.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "No schedules found for this learner."
                    };
                }

                // Get class IDs for fetching class days
                var classIds = schedules
                    .Where(s => s.ClassId.HasValue)
                    .Select(s => s.ClassId.Value)
                    .Distinct()
                    .ToList();

                // Fetch class days for these classes
                var classDays = await _unitOfWork.ClassDayRepository.GetQuery()
                    .Where(cd => classIds.Contains(cd.ClassId))
                    .ToListAsync();

                // Group class days by class ID for easy lookup
                var classDaysByClass = classDays
                    .GroupBy(cd => cd.ClassId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Map to DTOs with all the required information
                var scheduleDTOs = schedules.Select(schedule => {
                    var dto = _mapper.Map<ScheduleDTO>(schedule);

                    // Set DayOfWeek
                    dto.DayOfWeek = schedule.StartDay.DayOfWeek.ToString();

                    // Set LearnerAddress
                    dto.LearnerAddress = schedule.Learner?.Account?.Address;

                    // Set RegistrationStartDay from the Registration
                    dto.RegistrationStartDay = schedule.Registration?.StartDay;

                    // Set ScheduleDays from classDays if we have a ClassId
                    if (schedule.ClassId.HasValue && classDaysByClass.TryGetValue(schedule.ClassId.Value, out var days))
                    {
                        dto.ScheduleDays = days.Select(cd => new ScheduleDaysDTO
                        {
                            DayOfWeeks = cd.Day
                        }).ToList();

                        // Also set classDayDTOs
                        dto.classDayDTOs = days.Select(cd => new ClassDayDTO
                        {
                            Day = cd.Day
                        }).ToList();
                    }
                    else
                    {
                        dto.ScheduleDays = new List<ScheduleDaysDTO>();
                        dto.classDayDTOs = new List<ClassDayDTO>();
                    }

                    return dto;
                }).ToList();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Schedules retrieved successfully.",
                    Data = scheduleDTOs
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

        public async Task<ResponseDTO> GetClassAttendanceAsync(int classId)
        {
            var attendance = await _unitOfWork.ScheduleRepository.GetClassAttendanceAsync(classId);

            if (attendance == null || !attendance.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "No attendance records found for this class."
                };
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Attendance records retrieved successfully.",
                Data = attendance
            };
        }

        public async Task<ResponseDTO> GetOneOnOneAttendanceAsync(int learnerId)
        {
            var attendance = await _unitOfWork.ScheduleRepository.GetOneOnOneAttendanceAsync(learnerId);

            if (attendance == null || !attendance.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "No attendance records found for this learner."
                };
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Attendance records retrieved successfully.",
                Data = attendance
            };
        }


        public async Task<ResponseDTO> UpdateAttendanceAsync(int scheduleId, AttendanceStatus status)
        {
            var schedule = await _unitOfWork.ScheduleRepository.GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Schedule not found."
                };
            }

            // Get current date as DateOnly
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            // Check if the schedule's date is equal to today
            if (schedule.StartDay != today)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Attendance can only be updated for today's schedule."
                };
            }

            schedule.AttendanceStatus = status;
            await _unitOfWork.ScheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Attendance updated successfully."
            };
        }

        public async Task<ResponseDTO> CheckLearnerScheduleConflictAsync(int learnerId, DateOnly startDay, TimeOnly timeStart, int durationMinutes)
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

                // Calculate the end time for this session
                TimeOnly timeEnd = timeStart.AddMinutes(durationMinutes);

                // Get all existing registrations for this learner on the same day
                var existingRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetQuery()
                    .Where(r =>
                        r.LearnerId == learnerId &&
                        r.StartDay == startDay &&
                        (r.Status == LearningRegis.Pending ||
                         r.Status == LearningRegis.Accepted ||
                         r.Status == LearningRegis.Fourty ||
                         r.Status == LearningRegis.Sixty))
                    .ToListAsync();

                // Check for overlaps with existing registrations
                foreach (var registration in existingRegistrations)
                {
                    TimeOnly existingStart = registration.TimeStart;
                    TimeOnly existingEnd = registration.TimeStart.AddMinutes(registration.TimeLearning);

                    // Check for any kind of overlap
                    bool hasOverlap = (timeStart < existingEnd && existingStart < timeEnd);

                    if (hasOverlap)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = $"Schedule conflict detected. You already have a session from {existingStart:HH:mm} to {existingEnd:HH:mm} on this day. Please choose a different time slot.",
                            Data = new
                            {
                                ExistingStart = existingStart.ToString("HH:mm"),
                                ExistingEnd = existingEnd.ToString("HH:mm"),
                                RequestedStart = timeStart.ToString("HH:mm"),
                                RequestedEnd = timeEnd.ToString("HH:mm")
                            }
                        };
                    }
                }

                var (hasConflict, conflictingSchedules) = await _unitOfWork.ScheduleRepository
                    .CheckLearnerScheduleConflictAsync(learnerId, startDay, timeStart, durationMinutes);

                if (hasConflict)
                {
                    var conflictDetails = conflictingSchedules.Select(s => new
                    {
                        ScheduleId = s.ScheduleId,
                        StartDay = s.StartDay.ToString("yyyy-MM-dd"),
                        TimeStart = s.TimeStart.ToString("HH:mm"),
                        TimeEnd = s.TimeEnd.ToString("HH:mm"),
                        TeacherName = s.Teacher?.Fullname ?? "N/A",
                        ClassName = s.Class?.ClassName ?? "One-on-One Session",
                        Mode = s.Mode
                    }).ToList();

                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Schedule conflict detected. The learner already has classes scheduled during this time.",
                        Data = conflictDetails
                    };
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "No scheduling conflicts found."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error checking schedule conflicts: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CheckLearnerClassScheduleConflictAsync(int learnerId, int classId)
        {
            try
            {
                if (learnerId <= 0 || classId <= 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Invalid learner ID or class ID."
                    };
                }

                // Get class information for clear error messages
                var classInfo = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
                if (classInfo == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Class not found."
                    };
                }

                var (hasConflict, conflictingSchedules) = await _unitOfWork.ScheduleRepository
                    .CheckLearnerClassScheduleConflictAsync(learnerId, classId);

                if (hasConflict)
                {
                    // Format conflicting schedules for display
                    var conflictDetails = conflictingSchedules.Select(s => new
                    {
                        ScheduleId = s.ScheduleId,
                        DayOfWeek = s.StartDay.DayOfWeek.ToString(),
                        TimeStart = s.TimeStart.ToString("HH:mm"),
                        TimeEnd = s.TimeEnd.ToString("HH:mm"),
                        TeacherName = s.Teacher?.Fullname ?? "N/A",
                        ClassName = s.Class?.ClassName ?? "One-on-One Session",
                        Mode = s.Mode,
                        ConflictDetails = $"{s.StartDay.DayOfWeek} {s.TimeStart:HH:mm}-{s.TimeEnd:HH:mm} conflicts with {classInfo.ClassName} {classInfo.ClassTime:HH:mm}-{classInfo.ClassTime.AddHours(2):HH:mm}"
                    }).ToList();

                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Schedule conflicts detected. The learner has existing classes that overlap with {classInfo.ClassName}.",
                        Data = null
                    };
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "No scheduling conflicts found."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error checking class schedule conflicts: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetLearnerAttendanceStatsAsync(int learnerId, int? learningRegisId = null, int? classId = null)
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

                // For tracking individual registration attendance
                if (learningRegisId.HasValue)
                {
                    var learningRegis = await _unitOfWork.LearningRegisRepository
                        .GetByIdAsync(learningRegisId.Value);

                    if (learningRegis == null)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Learning registration not found."
                        };
                    }

                    if (learningRegis.LearnerId != learnerId)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "This learning registration does not belong to the specified learner."
                        };
                    }

                    var oneOnOneSchedules = await _unitOfWork.ScheduleRepository
                        .GetWhereAsync(s => s.LearningRegisId == learningRegisId.Value &&
                                           s.LearnerId == learnerId &&
                                           s.Mode == ScheduleMode.OneOnOne);

                    int totalSessions = learningRegis.NumberOfSession;
                    int attendedSessions = oneOnOneSchedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present);
                    int absentSessions = oneOnOneSchedules.Count(s => s.AttendanceStatus == AttendanceStatus.Absent);
                    int pendingSessions = totalSessions - attendedSessions - absentSessions;
                    double attendanceRate = totalSessions > 0 ? (double)attendedSessions / totalSessions * 100 : 0;

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Attendance statistics retrieved successfully.",
                        Data = new
                        {
                            LearningRegisId = learningRegis.LearningRegisId,
                            LearnerName = learningRegis.Learner?.FullName ?? "N/A",
                            TeacherName = learningRegis.Teacher?.Fullname ?? "N/A",
                            TotalSessions = totalSessions,
                            AttendedSessions = attendedSessions,
                            AbsentSessions = absentSessions,
                            PendingSessions = pendingSessions,
                            AttendanceRate = Math.Round(attendanceRate, 2),
                            RegistrationType = "One-on-One"
                        }
                    };
                }
                // For tracking class attendance
                else if (classId.HasValue)
                {
                    var classEntity = await _unitOfWork.ClassRepository
                        .GetByIdAsync(classId.Value);

                    if (classEntity == null)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Class not found."
                        };
                    }

                    // Check if learner is enrolled in this class
                    var isEnrolled = await _unitOfWork.LearningRegisRepository
                        .AnyAsync(lr => lr.LearnerId == learnerId &&
                                       lr.ClassId == classId.Value &&
                                       (lr.Status == LearningRegis.Accepted));

                    if (!isEnrolled)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "The learner is not enrolled in this class."
                        };
                    }

                    var classSchedules = await _unitOfWork.ScheduleRepository
                        .GetWhereAsync(s => s.ClassId == classId.Value &&
                                           s.LearnerId == learnerId &&
                                           s.Mode == ScheduleMode.Center);

                    int totalDays = classEntity.totalDays;
                    int attendedDays = classSchedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present);
                    int absentDays = classSchedules.Count(s => s.AttendanceStatus == AttendanceStatus.Absent);
                    int pendingDays = totalDays - attendedDays - absentDays;
                    double attendanceRate = totalDays > 0 ? (double)attendedDays / totalDays * 100 : 0;

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Attendance statistics retrieved successfully.",
                        Data = new
                        {
                            ClassId = classEntity.ClassId,
                            ClassName = classEntity.ClassName,
                            LearnerName = await GetLearnerName(learnerId),
                            TeacherName = classEntity.Teacher?.Fullname ?? "N/A",
                            TotalDays = totalDays,
                            AttendedDays = attendedDays,
                            AbsentDays = absentDays,
                            PendingDays = pendingDays,
                            AttendanceRate = Math.Round(attendanceRate, 2),
                            RegistrationType = "Class"
                        }
                    };
                }
                // If no specific registration or class is specified, return summary for all
                else
                {
                    // Get all 1-1 registrations
                    var oneOnOneRegistrations = await _unitOfWork.LearningRegisRepository
                        .GetWithIncludesAsync(
                            lr => lr.LearnerId == learnerId &&
                                 lr.ClassId == null &&
                                 (lr.Status == LearningRegis.Fourty || lr.Status == LearningRegis.Sixty),
                            "Teacher");

                    // Get all class enrollments
                    var classEnrollments = await _unitOfWork.LearningRegisRepository
                        .GetWithIncludesAsync(
                            lr => lr.LearnerId == learnerId &&
                                 lr.ClassId != null &&
                                 (lr.Status == LearningRegis.Accepted),
                            "Classes,Classes.Teacher");

                    var registrationStats = new List<object>();

                    // Process one-on-one registrations
                    foreach (var regis in oneOnOneRegistrations)
                    {
                        var schedules = await _unitOfWork.ScheduleRepository
                            .GetWhereAsync(s => s.LearningRegisId == regis.LearningRegisId &&
                                               s.LearnerId == learnerId &&
                                               s.Mode == ScheduleMode.OneOnOne);

                        int totalSessions = regis.NumberOfSession;
                        int attendedSessions = schedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present);
                        int absentSessions = schedules.Count(s => s.AttendanceStatus == AttendanceStatus.Absent);
                        int pendingSessions = totalSessions - attendedSessions - absentSessions;
                        double attendanceRate = totalSessions > 0 ? (double)attendedSessions / totalSessions * 100 : 0;

                        registrationStats.Add(new
                        {
                            RegistrationId = regis.LearningRegisId,
                            TeacherName = regis.Teacher?.Fullname ?? "N/A",
                            TotalSessions = totalSessions,
                            AttendedSessions = attendedSessions,
                            AbsentSessions = absentSessions,
                            PendingSessions = pendingSessions,
                            AttendanceRate = Math.Round(attendanceRate, 2),
                            Type = "One-on-One"
                        });
                    }

                    // Process class enrollments
                    foreach (var enrollment in classEnrollments)
                    {
                        if (enrollment.ClassId.HasValue && enrollment.Classes != null)
                        {
                            var classSchedules = await _unitOfWork.ScheduleRepository
                                .GetWhereAsync(s => s.ClassId == enrollment.ClassId.Value &&
                                                   s.LearnerId == learnerId &&
                                                   s.Mode == ScheduleMode.Center);

                            int totalDays = enrollment.Classes.totalDays;
                            int attendedDays = classSchedules.Count(s => s.AttendanceStatus == AttendanceStatus.Present);
                            int absentDays = classSchedules.Count(s => s.AttendanceStatus == AttendanceStatus.Absent);
                            int pendingDays = totalDays - attendedDays - absentDays;
                            double attendanceRate = totalDays > 0 ? (double)attendedDays / totalDays * 100 : 0;

                            registrationStats.Add(new
                            {
                                ClassId = enrollment.ClassId.Value,
                                ClassName = enrollment.Classes.ClassName,
                                TeacherName = enrollment.Classes.Teacher?.Fullname ?? "N/A",
                                TotalDays = totalDays,
                                AttendedDays = attendedDays,
                                AbsentDays = absentDays,
                                PendingDays = pendingDays,
                                AttendanceRate = Math.Round(attendanceRate, 2),
                                Type = "Class"
                            });
                        }
                    }

                    // Calculate overall statistics
                    int totalRegistrationDays = registrationStats.Sum(item =>
                        ((dynamic)item).Type == "One-on-One"
                            ? ((dynamic)item).TotalSessions
                            : ((dynamic)item).TotalDays);

                    int totalAttended = registrationStats.Sum(item =>
                        ((dynamic)item).Type == "One-on-One"
                            ? ((dynamic)item).AttendedSessions
                            : ((dynamic)item).AttendedDays);

                    int totalAbsent = registrationStats.Sum(item =>
                        ((dynamic)item).Type == "One-on-One"
                            ? ((dynamic)item).AbsentSessions
                            : ((dynamic)item).AbsentDays);

                    double overallAttendanceRate = totalRegistrationDays > 0
                        ? (double)totalAttended / totalRegistrationDays * 100
                        : 0;

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Attendance statistics retrieved successfully.",
                        Data = new
                        {
                            LearnerName = await GetLearnerName(learnerId),
                            OverallStatistics = new
                            {
                                TotalDays = totalRegistrationDays,
                                TotalAttended = totalAttended,
                                TotalAbsent = totalAbsent,
                                OverallAttendanceRate = Math.Round(overallAttendanceRate, 2)
                            },
                            DetailedStatistics = registrationStats
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to retrieve attendance statistics: {ex.Message}"
                };
            }
        }

        private async Task<string> GetLearnerName(int learnerId)
        {
            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
            return learner?.FullName ?? "N/A";
        }

    }
}
