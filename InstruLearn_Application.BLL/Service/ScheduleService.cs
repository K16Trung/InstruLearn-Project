﻿using AutoMapper;
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
        private readonly IEmailService _emailService;

        public ScheduleService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
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
                        Message = "Không tìm thấy đăng ký học tập hoặc ngày bắt đầu bị thiếu."
                    };
                }

                var startDate = learningRegis.StartDay.Value.ToDateTime(TimeOnly.MinValue);
                var registrationStartDate = startDate;
                var schedules = new List<ScheduleDTO>();
                int sessionCount = 0;

                var orderedLearningDays = learningRegis.LearningRegistrationDay.OrderBy(day => day.DayOfWeek).ToList();

                var currentDayOfWeek = registrationStartDate.DayOfWeek;
                var learningDaysStartingFromRegistration = orderedLearningDays
                    .Where(day => (DayOfWeek)day.DayOfWeek >= currentDayOfWeek)
                    .Concat(orderedLearningDays.Where(day => (DayOfWeek)day.DayOfWeek < currentDayOfWeek))
                    .ToList();

                while (sessionCount < learningRegis.NumberOfSession)
                {
                    foreach (var learningDay in learningDaysStartingFromRegistration)
                    {
                        var nextSessionDate = GetNextSessionDate(registrationStartDate, (DayOfWeek)learningDay.DayOfWeek);

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

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy lịch học thành công.",
                    Data = schedules
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không thể lấy lịch học. " + ex.Message
                };
            }
        }

        private DateTime GetNextSessionDate(DateTime startDate, DayOfWeek dayOfWeek)
        {
            int daysUntilNextSession = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;
            return startDate.AddDays(daysUntilNextSession);
        }

        public async Task<ResponseDTO> GetSchedulesByLearnerIdAsync(int learnerId)
        {
            try
            {
                if (learnerId <= 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "ID học viên không hợp lệ."
                    };
                }

                var learningRegs = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => x.LearnerId == learnerId && (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty || x.Status == LearningRegis.FourtyFeedbackDone),
                        "Teacher,Learner.Account,LearningRegistrationDay,Schedules.Teacher,Major"
                    );

                if (learningRegs == null || !learningRegs.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy đăng ký học tập nào cho học viên này."
                    };
                }

                var schedules = new List<ScheduleDTO>();
                var scheduleRelationships = new List<object>();

                foreach (var learningRegis in learningRegs)
                {
                    if (!learningRegis.StartDay.HasValue)
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Thiếu ngày bắt đầu."
                        };
                    }

                    var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                        .GetByLearningRegisIdAsync(learningRegis.LearningRegisId);

                    var sortedSessions = learningPathSessions
                        .OrderBy(s => s.SessionNumber)
                        .ToList();

                    var existingSchedules = learningRegis.Schedules
                        .Where(s => s.Mode == ScheduleMode.OneOnOne)
                        .OrderBy(s => s.StartDay)
                        .ToList();

                    var makeupRelations = new Dictionary<int, int>();
                    var originalRelations = new Dictionary<int, int>();

                    foreach (var schedule in existingSchedules)
                    {
                        string changeReason = schedule.ChangeReason ?? "";

                        var originalMatch = System.Text.RegularExpressions.Regex.Match(changeReason, @"\[Original:(\d+)\]");
                        if (originalMatch.Success && int.TryParse(originalMatch.Groups[1].Value, out int originalId))
                        {
                            originalRelations[schedule.ScheduleId] = originalId;
                        }

                        var makeupMatch = System.Text.RegularExpressions.Regex.Match(changeReason, @"\[Makeup:(\d+)\]");
                        if (makeupMatch.Success && int.TryParse(makeupMatch.Groups[1].Value, out int makeupId))
                        {
                            makeupRelations[schedule.ScheduleId] = makeupId;
                        }
                    }

                    foreach (var existingSchedule in existingSchedules)
                    {
                        var sessionPosition = existingSchedule.PreferenceStatus == PreferenceStatus.MakeupClass
                            ? existingSchedules.Where(s => s.PreferenceStatus != PreferenceStatus.MakeupClass)
                                .OrderBy(s => s.StartDay)
                                .TakeWhile(s => s.StartDay < existingSchedule.StartDay)
                                .Count() + 1
                            : existingSchedules.Where(s => s.PreferenceStatus != PreferenceStatus.MakeupClass)
                                .OrderBy(s => s.StartDay)
                                .ToList()
                                .IndexOf(existingSchedule) + 1;

                        var sessionNumber = Math.Max(1, sessionPosition);
                        var learningPathSession = sortedSessions.FirstOrDefault(s => s.SessionNumber == sessionNumber);

                        DateOnly scheduleStartDate = existingSchedule.StartDay;
                        TimeOnly scheduleTimeStart = existingSchedule.TimeStart;
                        TimeOnly scheduleTimeEnd = existingSchedule.TimeEnd;

                        var orderedLearningDays = learningRegis.LearningRegistrationDay
                            .OrderBy(day => day.DayOfWeek)
                            .ToList();

                        var scheduleTeacherId = existingSchedule.TeacherId ?? learningRegis.TeacherId;
                        var scheduleTeacherName = existingSchedule.Teacher?.Fullname ??
                                                  learningRegis.Teacher?.Fullname ?? "N/A";

                        var scheduleDto = new ScheduleDTO
                        {
                            ScheduleId = existingSchedule.ScheduleId,
                            Mode = existingSchedule.Mode,
                            TimeStart = scheduleTimeStart.ToString("HH:mm"),
                            TimeEnd = scheduleTimeEnd.ToString("HH:mm"),
                            DayOfWeek = scheduleStartDate.DayOfWeek.ToString(),
                            StartDay = scheduleStartDate,
                            TeacherId = scheduleTeacherId,
                            TeacherName = scheduleTeacherName,
                            LearnerId = learningRegis.LearnerId,
                            LearnerName = learningRegis.Learner?.FullName ?? "N/A",
                            LearnerAddress = learningRegis.Learner?.Account?.Address ?? "N/A",
                            ClassId = learningRegis.ClassId,
                            ClassName = learningRegis.Classes?.ClassName ?? "N/A",
                            RegistrationStartDay = learningRegis.StartDay,
                            LearningRegisId = learningRegis.LearningRegisId,
                            AttendanceStatus = existingSchedule.AttendanceStatus,
                            ChangeReason = existingSchedule.ChangeReason,
                            PreferenceStatus = existingSchedule.PreferenceStatus,
                            MajorId = learningRegis.MajorId,
                            MajorName = learningRegis.Major?.MajorName ?? "N/A",
                            TimeLearning = learningRegis.TimeLearning,
                            ScheduleDays = orderedLearningDays.Select(day => new ScheduleDaysDTO
                            {
                                DayOfWeeks = (DayOfWeeks)day.DayOfWeek
                            }).ToList()
                        };

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
                            scheduleDto.SessionNumber = sessionNumber;
                            scheduleDto.SessionTitle = $"Session {sessionNumber}";
                            scheduleDto.SessionDescription = "";
                            scheduleDto.IsSessionCompleted = false;
                        }

                        if (existingSchedule.PreferenceStatus == PreferenceStatus.MakeupClass)
                        {
                            scheduleDto.IsMakeupClass = true;

                            if (originalRelations.TryGetValue(existingSchedule.ScheduleId, out int originalId))
                            {
                                scheduleRelationships.Add(new
                                {
                                    MakeupScheduleId = existingSchedule.ScheduleId,
                                    OriginalScheduleId = originalId
                                });
                            }
                        }

                        if (makeupRelations.TryGetValue(existingSchedule.ScheduleId, out int makeupId))
                        {
                            scheduleRelationships.Add(new
                            {
                                OriginalScheduleId = existingSchedule.ScheduleId,
                                MakeupScheduleId = makeupId
                            });
                        }

                        schedules.Add(scheduleDto);
                    }
                }

                object responseData = scheduleRelationships.Any()
                    ? new { Schedules = schedules, ScheduleRelationships = scheduleRelationships }
                    : (object)schedules;

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy lịch học thành công.",
                    Data = responseData
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không thể lấy lịch học: {ex.Message}"
                };
            }
        }


        public async Task<ResponseDTO> GetSchedulesByTeacherIdAsync(int teacherId)
        {
            try
            {
                if (teacherId <= 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "ID giáo viên không hợp lệ."
                    };
                }

                var teacherSchedules = await _unitOfWork.ScheduleRepository
                    .GetWhereAsync(s => s.TeacherId == teacherId && s.Mode == ScheduleMode.OneOnOne);

                var learningRegisIds = teacherSchedules
                    .Where(s => s.LearningRegisId.HasValue)
                    .Select(s => s.LearningRegisId.Value)
                    .Distinct()
                    .ToList();

                var learningRegs = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        x => learningRegisIds.Contains(x.LearningRegisId) &&
                             (x.Status == LearningRegis.Fourty || x.Status == LearningRegis.Sixty || x.Status == LearningRegis.FourtyFeedbackDone),
                        "Teacher,Learner.Account,LearningRegistrationDay,Schedules.Teacher,Major"
                    );

                if (!learningRegs.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy đăng ký học tập nào cho giáo viên này."
                    };
                }

                var schedules = new List<ScheduleDTO>();
                var scheduleRelationships = new List<object>();

                foreach (var learningRegis in learningRegs)
                {
                    if (!learningRegis.StartDay.HasValue)
                    {
                        continue;
                    }

                    var learningPathSessions = await _unitOfWork.LearningPathSessionRepository
                        .GetByLearningRegisIdAsync(learningRegis.LearningRegisId);

                    var sortedSessions = learningPathSessions
                        .OrderBy(s => s.SessionNumber)
                        .ToList();

                    var allRegistrationSchedules = learningRegis.Schedules
                        .Where(s => s.Mode == ScheduleMode.OneOnOne)
                        .OrderBy(s => s.StartDay)
                        .ToList();

                    var existingSchedules = learningRegis.Schedules
                        .Where(s => s.Mode == ScheduleMode.OneOnOne && s.TeacherId == teacherId)
                        .OrderBy(s => s.StartDay)
                        .ToList();

                    var makeupRelations = new Dictionary<int, int>();
                    var originalRelations = new Dictionary<int, int>();

                    foreach (var schedule in existingSchedules)
                    {
                        string changeReason = schedule.ChangeReason ?? "";

                        var originalMatch = System.Text.RegularExpressions.Regex.Match(changeReason, @"\[Original:(\d+)\]");
                        if (originalMatch.Success && int.TryParse(originalMatch.Groups[1].Value, out int originalId))
                        {
                            originalRelations[schedule.ScheduleId] = originalId;
                        }

                        var makeupMatch = System.Text.RegularExpressions.Regex.Match(changeReason, @"\[Makeup:(\d+)\]");
                        if (makeupMatch.Success && int.TryParse(makeupMatch.Groups[1].Value, out int makeupId))
                        {
                            makeupRelations[schedule.ScheduleId] = makeupId;
                        }
                    }

                    foreach (var existingSchedule in existingSchedules)
                    {
                        DateOnly scheduleStartDate = existingSchedule.StartDay;

                        var allSchedulesPosition = existingSchedule.PreferenceStatus == PreferenceStatus.MakeupClass
                            ? allRegistrationSchedules.Where(s => s.PreferenceStatus != PreferenceStatus.MakeupClass)
                                .OrderBy(s => s.StartDay)
                                .TakeWhile(s => s.StartDay < existingSchedule.StartDay)
                                .Count()
                            : allRegistrationSchedules
                                .FindIndex(s => s.ScheduleId == existingSchedule.ScheduleId);

                        var sessionNumber = Math.Max(1, allSchedulesPosition + 1);
                        var learningPathSession = sortedSessions.FirstOrDefault(s => s.SessionNumber == sessionNumber);

                        var scheduleExists = schedules.Any(s =>
                            s.StartDay == scheduleStartDate &&
                            s.LearningRegisId == learningRegis.LearningRegisId &&
                            s.ScheduleId == existingSchedule.ScheduleId);

                        if (!scheduleExists)
                        {
                            var orderedLearningDays = learningRegis.LearningRegistrationDay
                                .OrderBy(day => day.DayOfWeek)
                                .ToList();

                            var scheduleDto = new ScheduleDTO
                            {
                                ScheduleId = existingSchedule.ScheduleId,
                                Mode = existingSchedule.Mode,
                                TimeStart = existingSchedule.TimeStart.ToString("HH:mm"),
                                TimeEnd = existingSchedule.TimeEnd.ToString("HH:mm"),
                                DayOfWeek = scheduleStartDate.DayOfWeek.ToString(),
                                StartDay = scheduleStartDate,
                                TeacherId = teacherId,
                                TeacherName = existingSchedule.Teacher?.Fullname ?? "N/A",
                                LearnerId = learningRegis.LearnerId,
                                LearnerName = learningRegis.Learner?.FullName ?? "N/A",
                                LearnerAddress = learningRegis.Learner?.Account?.Address ?? "N/A",
                                ClassId = learningRegis.ClassId,
                                ClassName = learningRegis.Classes?.ClassName ?? "N/A",
                                RegistrationStartDay = learningRegis.StartDay,
                                LearningRegisId = learningRegis.LearningRegisId,
                                AttendanceStatus = existingSchedule.AttendanceStatus,
                                ChangeReason = existingSchedule.ChangeReason,
                                PreferenceStatus = existingSchedule.PreferenceStatus,
                                MajorId = learningRegis.MajorId,
                                MajorName = learningRegis.Major?.MajorName ?? "N/A",
                                TimeLearning = learningRegis.TimeLearning,
                                ScheduleDays = orderedLearningDays.Select(day => new ScheduleDaysDTO
                                {
                                    DayOfWeeks = (DayOfWeeks)day.DayOfWeek
                                }).ToList()
                            };

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
                                scheduleDto.SessionNumber = sessionNumber;
                                scheduleDto.SessionTitle = $"Session {sessionNumber}";
                                scheduleDto.SessionDescription = "";
                                scheduleDto.IsSessionCompleted = false;
                            }

                            if (existingSchedule.PreferenceStatus == PreferenceStatus.MakeupClass)
                            {
                                scheduleDto.IsMakeupClass = true;

                                if (originalRelations.TryGetValue(existingSchedule.ScheduleId, out int originalId))
                                {
                                    scheduleRelationships.Add(new
                                    {
                                        MakeupScheduleId = existingSchedule.ScheduleId,
                                        OriginalScheduleId = originalId
                                    });
                                }
                            }

                            if (makeupRelations.TryGetValue(existingSchedule.ScheduleId, out int makeupId))
                            {
                                scheduleRelationships.Add(new
                                {
                                    OriginalScheduleId = existingSchedule.ScheduleId,
                                    MakeupScheduleId = makeupId
                                });
                            }

                            schedules.Add(scheduleDto);
                        }
                    }
                }

                object responseData = scheduleRelationships.Any()
                    ? new { Schedules = schedules, ScheduleRelationships = scheduleRelationships }
                    : (object)schedules;

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã lấy lịch dạy thành công.",
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
                Message = "Đã lấy lịch dạy thành công.",
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


        public async Task<List<ValidTeacherDTO>> GetAvailableTeachersAsync(int majorId, TimeOnly timeStart, int timeLearning, DateOnly[] startDay)
        {
            var freeTeacherIds = await _unitOfWork.ScheduleRepository.GetFreeTeacherIdsAsync(majorId, timeStart, timeLearning, startDay);

            if (!freeTeacherIds.Any())
            {
                return new List<ValidTeacherDTO>();
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
                        Message = "ID học viên không hợp lệ."
                    };
                }

                var schedules = await _unitOfWork.ScheduleRepository.GetClassSchedulesByLearnerIdAsync(learnerId);

                if (schedules == null || !schedules.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy lịch học nào cho học viên này."
                    };
                }

                var classIds = schedules
                    .Where(s => s.ClassId.HasValue)
                    .Select(s => s.ClassId.Value)
                    .Distinct()
                    .ToList();

                var classDays = await _unitOfWork.ClassDayRepository.GetQuery()
                    .Where(cd => classIds.Contains(cd.ClassId))
                    .ToListAsync();

                var classDaysByClass = classDays
                    .GroupBy(cd => cd.ClassId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var scheduleDTOs = schedules.Select(schedule => {
                    var dto = _mapper.Map<ScheduleDTO>(schedule);

                    dto.DayOfWeek = schedule.StartDay.DayOfWeek.ToString();

                    dto.LearnerAddress = schedule.Learner?.Account?.Address;

                    dto.RegistrationStartDay = schedule.Registration?.StartDay;

                    if (schedule.ClassId.HasValue && classDaysByClass.TryGetValue(schedule.ClassId.Value, out var days))
                    {
                        dto.ScheduleDays = days.Select(cd => new ScheduleDaysDTO
                        {
                            DayOfWeeks = cd.Day
                        }).ToList();

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
                    Message = "Đã lấy lịch học thành công.",
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
                    Message = "Không tìm thấy bản ghi điểm danh nào cho lớp học này."
                };
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã lấy bản ghi điểm danh thành công.",
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
                    Message = "Không tìm thấy bản ghi điểm danh nào cho học viên này."
                };
            }

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã lấy bản ghi điểm danh thành công.",
                Data = attendance
            };
        }


        public async Task<ResponseDTO> UpdateAttendanceAsync(int scheduleId, AttendanceStatus status, PreferenceStatus preferenceStatus)
        {
            var schedule = await _unitOfWork.ScheduleRepository.GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy lịch học."
                };
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            if (schedule.StartDay != today)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Attendance can only be updated for today's schedule."
                };
            }

            schedule.AttendanceStatus = status;
            schedule.PreferenceStatus = preferenceStatus;
            await _unitOfWork.ScheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Attendance updated successfully."
            };
        }

        public async Task<ResponseDTO> CheckClassAttendanceAsync(int scheduleId, AttendanceStatus status, DateOnly currentDate)
        {
            try
            {
                var schedule = await _unitOfWork.ScheduleRepository.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
                if (schedule == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Schedule not found"
                    };
                }

                if (schedule.StartDay != currentDate)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Attendance can only be updated for today's sessions."
                    };
                }

                schedule.AttendanceStatus = status;
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Attendance status updated successfully",
                    Data = new
                    {
                        ScheduleId = schedule.ScheduleId,
                        AttendanceStatus = schedule.AttendanceStatus
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error updating attendance status: {ex.Message}"
                };
            }
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

                TimeOnly timeEnd = timeStart.AddMinutes(durationMinutes);

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

                foreach (var registration in existingRegistrations)
                {
                    TimeOnly existingStart = registration.TimeStart;
                    TimeOnly existingEnd = registration.TimeStart.AddMinutes(registration.TimeLearning);

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
                        Message = $"Trùng lịch học. Lịch học của học viên đã trùng với {classInfo.ClassName}.",
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

        public async Task<ResponseDTO> GetLearnerAttendanceStatsAsync(int learnerId)
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

                var oneOnOneRegistrations = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        lr => lr.LearnerId == learnerId &&
                             lr.ClassId == null &&
                             (lr.Status == LearningRegis.Fourty ||
                              lr.Status == LearningRegis.Sixty ||
                              lr.Status == LearningRegis.FourtyFeedbackDone),
                        "Teacher,Major");

                var classEnrollments = await _unitOfWork.LearningRegisRepository
                    .GetWithIncludesAsync(
                        lr => lr.LearnerId == learnerId &&
                             lr.ClassId != null &&
                             (lr.Status == LearningRegis.Accepted),
                        "Classes,Classes.Teacher,Major");

                var registrationStats = new List<object>();

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
                        MajorName = regis.Major?.MajorName ?? "N/A",
                        TotalSessions = totalSessions,
                        AttendedSessions = attendedSessions,
                        AbsentSessions = absentSessions,
                        PendingSessions = pendingSessions,
                        AttendanceRate = Math.Round(attendanceRate, 2),
                        LearningRequest = regis.LearningRequest,
                        Status = regis.Status.ToString(),
                        Type = "One-on-One"
                    });
                }

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
                            MajorName = enrollment.Major?.MajorName ?? "N/A",
                            TotalDays = totalDays,
                            AttendedDays = attendedDays,
                            AbsentDays = absentDays,
                            PendingDays = pendingDays,
                            AttendanceRate = Math.Round(attendanceRate, 2),
                            LearningRequest = enrollment.LearningRequest,
                            Status = enrollment.Status.ToString(),
                            Type = "Class"
                        });
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Attendance statistics retrieved successfully.",
                    Data = new
                    {
                        LearnerName = await GetLearnerName(learnerId),
                        RegistrationStatistics = registrationStats
                    }
                };
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


        public async Task<ResponseDTO> UpdateScheduleTeacherAsync(int scheduleId, int teacherId, string changeReason)
        {
            try
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

                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Teacher not found."
                    };
                }

                var oldTeacherId = schedule.TeacherId;
                var oldTeacherName = oldTeacherId.HasValue
                    ? (await _unitOfWork.TeacherRepository.GetByIdAsync(oldTeacherId.Value))?.Fullname ?? "Unknown"
                    : "No teacher";

                schedule.TeacherId = teacherId;
                schedule.ChangeReason = changeReason;
                schedule.PreferenceStatus = PreferenceStatus.ChangeTeacher;

                await _unitOfWork.ScheduleRepository.UpdateAsync(schedule);
                await _unitOfWork.SaveChangeAsync();

                string learnerName = "N/A";
                if (schedule.LearnerId.HasValue)
                {
                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(schedule.LearnerId.Value);
                    learnerName = learner?.FullName ?? "N/A";
                }

                string formattedDate = schedule.StartDay.ToString("dd/MM/yyyy");
                string formattedStartTime = schedule.TimeStart.ToString("HH:mm");
                string formattedEndTime = schedule.TimeEnd.ToString("HH:mm");

                if (schedule.LearnerId.HasValue)
                {
                    var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(schedule.LearnerId.Value);
                    if (learner != null && learner.Account != null)
                    {
                        var account = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
                        if (account != null && !string.IsNullOrEmpty(account.Email))
                        {
                            string subject = "Lịch học của bạn đã được cập nhật";
                            string body = $@"
                <html>
                <body>
                    <h2>Thông báo cập nhật lịch học</h2>
                    <p>Xin chào {learner.FullName},</p>
                    <p>Chúng tôi muốn thông báo rằng lịch học của bạn ngày {formattedDate} từ {formattedStartTime} - {formattedEndTime} đã được thay đổi.</p>
                    <p><strong>Thay đổi:</strong> Giáo viên hiện tại của bạn đã được thay thế từ {oldTeacherName} sang {teacher.Fullname}.</p>
                    <p><strong>Lý do:</strong> {changeReason}</p>
                    <p>Nếu bạn có bất kì câu hỏi nào, vui lòng liên hệ với nhóm hỗ trợ của chúng tôi.</p>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ InstruLearn!</p>
                    <p>Trân trọng,<br/>The InstruLearn Team</p>
                </body>
                </html>";

                            await _emailService.SendEmailAsync(account.Email, subject, body);
                        }
                    }
                }

                if (teacher.AccountId != null)
                {
                    var teacherAccount = await _unitOfWork.AccountRepository.GetByIdAsync(teacher.AccountId);
                    if (teacherAccount != null && !string.IsNullOrEmpty(teacherAccount.Email))
                    {
                        string subject = "New Schedule Assignment";
                        string body = $@"
                <html>
                <body>
                    <h2>Thông báo lịch dạy mới</h2>
                    <p>Xin chào {teacher.Fullname},</p>
                    <p>Bạn đã được phân công một lịch dạy mới như sau:</p>
                    <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4CAF50;'>
                        <h3 style='margin-top: 0; color: #333;'>Chi tiết lịch học:</h3>
                        <p><strong>Ngày:</strong> {formattedDate}</p>
                        <p><strong>Thời gian:</strong> {formattedStartTime} - {formattedEndTime}</p>
                        <p><strong>Học viên:</strong> {learnerName}</p>
                        <p><strong>Lý do được phân công:</strong> {changeReason}</p>
                    </div>
                    <p>Vui lòng kiểm tra hệ thống để biết thêm chi tiết về lịch dạy của bạn.</p>
                    <p>Nếu bạn có bất kì câu hỏi nào, vui lòng liên hệ với quản trị viên.</p>
                    <p>Trân trọng,<br/>The InstruLearn Team</p>
                </body>
                </html>";

                        await _emailService.SendEmailAsync(teacherAccount.Email, subject, body);
                    }
                }

                if (oldTeacherId.HasValue && oldTeacherId.Value != teacherId)
                {
                    var oldTeacher = await _unitOfWork.TeacherRepository.GetByIdAsync(oldTeacherId.Value);
                    if (oldTeacher != null && oldTeacher.AccountId != null)
                    {
                        var oldTeacherAccount = await _unitOfWork.AccountRepository.GetByIdAsync(oldTeacher.AccountId);
                        if (oldTeacherAccount != null && !string.IsNullOrEmpty(oldTeacherAccount.Email))
                        {
                            string subject = "Schedule Assignment Change";
                            string body = $@"
                    <html>
                    <body>
                        <h2>Thông báo thay đổi lịch dạy</h2>
                        <p>Xin chào {oldTeacher.Fullname},</p>
                        <p>Một lịch dạy của bạn đã được chuyển cho giáo viên khác:</p>
                        <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
                            <h3 style='margin-top: 0; color: #333;'>Chi tiết lịch học bị thay đổi:</h3>
                            <p><strong>Ngày:</strong> {formattedDate}</p>
                            <p><strong>Thời gian:</strong> {formattedStartTime} - {formattedEndTime}</p>
                            <p><strong>Học viên:</strong> {learnerName}</p>
                            <p><strong>Giáo viên mới:</strong> {teacher.Fullname}</p>
                            <p><strong>Lý do thay đổi:</strong> {changeReason}</p>
                        </div>
                        <p>Vui lòng kiểm tra hệ thống để cập nhật lịch dạy mới của bạn.</p>
                        <p>Nếu bạn có bất kì câu hỏi nào, vui lòng liên hệ với quản trị viên.</p>
                        <p>Trân trọng,<br/>The InstruLearn Team</p>
                    </body>
                    </html>";

                            await _emailService.SendEmailAsync(oldTeacherAccount.Email, subject, body);
                        }
                    }
                }

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Schedule updated successfully. Notifications sent to the learner and teachers.",
                    Data = new
                    {
                        ScheduleId = schedule.ScheduleId,
                        TeacherId = schedule.TeacherId,
                        TeacherName = teacher.Fullname,
                        PreviousTeacherId = oldTeacherId,
                        PreviousTeacherName = oldTeacherName,
                        ChangeReason = changeReason
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to update schedule: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateScheduleForMakeupAsync(int scheduleId, DateOnly newDate, TimeOnly newTimeStart, int timeLearning, string changeReason)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var originalSchedule = await _unitOfWork.ScheduleRepository.GetByIdAsync(scheduleId);
                if (originalSchedule == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Schedule not found."
                    };
                }

                if (originalSchedule.AttendanceStatus != AttendanceStatus.Absent)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Only schedules marked as 'Absent' can be changed for makeup classes."
                    };
                }

                var existingMakeupSchedules = await _unitOfWork.ScheduleRepository.GetWhereAsync(
                    s => s.LearningRegisId == originalSchedule.LearningRegisId &&
                        s.PreferenceStatus == PreferenceStatus.MakeupClass);

                bool isUpdating = false;
                Schedules makeupSchedule = null;

                if (originalSchedule.PreferenceStatus == PreferenceStatus.MakeupClass)
                {
                    if (!existingMakeupSchedules.Any())
                    {
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "The schedule is marked for makeup but no makeup schedule was found."
                        };
                    }

                    makeupSchedule = existingMakeupSchedules.First();
                    isUpdating = true;
                }
                else if (existingMakeupSchedules.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "A makeup class has already been scheduled for this learning registration. Only one makeup class is allowed."
                    };
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                if (newDate < today)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Makeup class must be scheduled for a future date."
                    };
                }

                if (!isUpdating && newDate <= originalSchedule.StartDay)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Makeup class must be scheduled after the date of the absent class."
                    };
                }

                if (!isUpdating && originalSchedule.StartDay.DayOfWeek == newDate.DayOfWeek)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Makeup class must be scheduled on a different day of the week. Original class was on {originalSchedule.StartDay.DayOfWeek}."
                    };
                }

                var learner = originalSchedule.LearnerId.HasValue
                    ? await _unitOfWork.LearnerRepository.GetByIdAsync(originalSchedule.LearnerId.Value)
                    : null;

                var learnerName = learner?.FullName ?? "N/A";

                if (originalSchedule.LearnerId.HasValue)
                {
                    var conflictCheck = await CheckLearnerScheduleConflictAsync(
                        originalSchedule.LearnerId.Value,
                        newDate,
                        newTimeStart,
                        timeLearning);

                    if (!conflictCheck.IsSucceed && isUpdating)
                    {
                        if (conflictCheck.Data is List<object> conflicts)
                        {
                            var filteredConflicts = conflicts
                                .Cast<dynamic>()
                                .Where(c => (int)c.ScheduleId != makeupSchedule.ScheduleId)
                                .ToList();

                            if (filteredConflicts.Any())
                            {
                                return new ResponseDTO
                                {
                                    IsSucceed = false,
                                    Message = "Schedule conflict detected. The learner already has classes scheduled during this time.",
                                    Data = filteredConflicts
                                };
                            }
                        }
                        else
                        {
                            return conflictCheck;
                        }
                    }
                    else if (!conflictCheck.IsSucceed)
                    {
                        return conflictCheck;
                    }
                }

                var oldDate = originalSchedule.StartDay;
                var oldTimeStart = originalSchedule.TimeStart;
                var oldTimeEnd = originalSchedule.TimeEnd;
                var oldDayOfWeek = oldDate.DayOfWeek;

                DateOnly currentMakeupDate = DateOnly.MinValue;
                TimeOnly currentMakeupTimeStart = TimeOnly.MinValue;
                TimeOnly currentMakeupTimeEnd = TimeOnly.MinValue;

                if (isUpdating)
                {
                    currentMakeupDate = makeupSchedule.StartDay;
                    currentMakeupTimeStart = makeupSchedule.TimeStart;
                    currentMakeupTimeEnd = makeupSchedule.TimeEnd;
                }

                if (isUpdating)
                {
                    makeupSchedule.StartDay = newDate;
                    makeupSchedule.TimeStart = newTimeStart;
                    makeupSchedule.TimeEnd = newTimeStart.AddMinutes(timeLearning);
                    makeupSchedule.ChangeReason = $"{changeReason} (Updated: {DateTime.Now:yyyy-MM-dd HH:mm})";

                    makeupSchedule.AttendanceStatus = AttendanceStatus.NotYet;
                    makeupSchedule.PreferenceStatus = PreferenceStatus.None;

                    await _unitOfWork.ScheduleRepository.UpdateAsync(makeupSchedule);
                }
                else
                {
                    makeupSchedule = new Schedules
                    {
                        TeacherId = originalSchedule.TeacherId,
                        LearnerId = originalSchedule.LearnerId,
                        LearningRegisId = originalSchedule.LearningRegisId,
                        ClassId = originalSchedule.ClassId,
                        StartDay = newDate,
                        TimeStart = newTimeStart,
                        TimeEnd = newTimeStart.AddMinutes(timeLearning),
                        Mode = originalSchedule.Mode,
                        AttendanceStatus = AttendanceStatus.NotYet,
                        ChangeReason = changeReason,
                        PreferenceStatus = PreferenceStatus.MakeupClass
                    };

                    await _unitOfWork.ScheduleRepository.AddAsync(makeupSchedule);

                    await _unitOfWork.SaveChangeAsync();

                    originalSchedule.PreferenceStatus = PreferenceStatus.MakeupClass;
                    string originalReason = string.IsNullOrEmpty(originalSchedule.ChangeReason)
                        ? "" : originalSchedule.ChangeReason + " | ";
                    originalSchedule.ChangeReason = $"{originalReason}Makeup scheduled for {newDate:yyyy-MM-dd} [Makeup:{makeupSchedule.ScheduleId}]";
                    await _unitOfWork.ScheduleRepository.UpdateAsync(originalSchedule);
                }

                await _unitOfWork.SaveChangeAsync();

                string oldFormattedDate = oldDate.ToString("dd/MM/yyyy");
                string oldFormattedTimeStart = oldTimeStart.ToString("HH:mm");
                string oldFormattedTimeEnd = oldTimeEnd.ToString("HH:mm");

                string newFormattedDate = newDate.ToString("dd/MM/yyyy");
                string newFormattedTimeStart = newTimeStart.ToString("HH:mm");
                string newFormattedTimeEnd = makeupSchedule.TimeEnd.ToString("HH:mm");

                string emailSubject = isUpdating ? "Your Makeup Class Has Been Rescheduled" : "Your Makeup Class Has Been Scheduled";
                string emailIntro = isUpdating ?
                    "Chúng tôi xác nhận lịch học bù của bạn đã được thay đổi lại." :
                    "Chúng tôi xác nhận lịch học bù của bạn đã được xếp.";

                if (originalSchedule.LearnerId.HasValue && learner?.Account != null && !string.IsNullOrEmpty(learner.Account.Email))
                {
                    string subject = emailSubject;
                    string body = $@"
<html>
<body>
    <h2>Lịch học bù đã được xác nhận</h2>
    <p>Xin chào {learnerName},</p>
    <p>{emailIntro}</p>
    
    <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px;'>
        <h3>Thông tin lịch học cũ:</h3>
        <p><strong>Ngày:</strong> {oldFormattedDate} ({oldDayOfWeek})</p>
        <p><strong>Thời gian:</strong> {oldFormattedTimeStart} - {oldFormattedTimeEnd}</p>
    </div>
    
    {(isUpdating ? $@"
    <div style='background-color: #f5f0e8; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
        <h3>Thông tin lịch học bù hiện tại:</h3>
        <p><strong>Ngày:</strong> {currentMakeupDate:dd/MM/yyyy} ({currentMakeupDate.DayOfWeek})</p>
        <p><strong>Thời gian:</strong> {currentMakeupTimeStart:HH:mm} - {currentMakeupTimeEnd:HH:mm}</p>
    </div>
    " : "")}
    
    <div style='background-color: #e8f5e9; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4CAF50;'>
        <h3>Thông tin lịch học {(isUpdating ? "bù mới" : "bù")}:</h3>
        <p><strong>Ngày:</strong> {newFormattedDate} ({newDate.DayOfWeek})</p>
        <p><strong>Thời gian:</strong> {newFormattedTimeStart} - {newFormattedTimeEnd}</p>
        <p><strong>Thời gian học:</strong> {timeLearning} phút</p>
        <p><strong>Lý do thay đổi:</strong> {changeReason}</p>
    </div>
    
    <p>Vui lòng đảm bảo bạn có mặt đúng giờ cho buổi học bù này.</p>
    <p>Nếu bạn có bất kì câu hỏi nào, vui lòng liên hệ với nhóm hỗ trợ của chúng tôi.</p>
    <p>Cảm ơn bạn đã sử dụng dịch vụ InstruLearn!</p>
    <p>Trân trọng,<br/>The InstruLearn Team</p>
</body>
</html>";

                    await _emailService.SendEmailAsync(learner.Account.Email, subject, body);
                }

                if (originalSchedule.TeacherId.HasValue)
                {
                    var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(originalSchedule.TeacherId.Value);

                    if (teacher?.AccountId != null)
                    {
                        var teacherAccount = await _unitOfWork.AccountRepository.GetByIdAsync(teacher.AccountId);

                        if (teacherAccount != null && !string.IsNullOrEmpty(teacherAccount.Email))
                        {
                            string subject = isUpdating ? "Makeup Class Schedule Has Been Updated" : "Makeup Class Schedule";
                            string body = $@"
<html>
<body>
    <h2>Thông báo lịch dạy bù</h2>
    <p>Xin chào {teacher.Fullname},</p>
    <p>Lịch dạy bù {(isUpdating ? "đã được cập nhật lại" : "đã được xếp")} như sau:</p>
    
    <div style='background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px;'>
        <h3>Thông tin lịch dạy cũ:</h3>
        <p><strong>Ngày:</strong> {oldFormattedDate} ({oldDayOfWeek})</p>
        <p><strong>Thời gian:</strong> {oldFormattedTimeStart} - {oldFormattedTimeEnd}</p>
        <p><strong>Học viên:</strong> {learnerName}</p>
    </div>
    
    {(isUpdating ? $@"
    <div style='background-color: #f5f0e8; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #ff9800;'>
        <h3>Thông tin lịch dạy bù hiện tại:</h3>
        <p><strong>Ngày:</strong> {currentMakeupDate:dd/MM/yyyy} ({currentMakeupDate.DayOfWeek})</p>
        <p><strong>Thời gian:</strong> {currentMakeupTimeStart:HH:mm} - {currentMakeupTimeEnd:HH:mm}</p>
    </div>
    " : "")}
    
    <div style='background-color: #e8f5e9; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #4CAF50;'>
        <h3>Thông tin lịch dạy {(isUpdating ? "bù mới" : "bù")}:</h3>
        <p><strong>Ngày:</strong> {newFormattedDate} ({newDate.DayOfWeek})</p>
        <p><strong>Thời gian:</strong> {newFormattedTimeStart} - {newFormattedTimeEnd}</p>
        <p><strong>Thời gian dạy:</strong> {timeLearning} phút</p>
        <p><strong>Học viên:</strong> {learnerName}</p>
        <p><strong>Lý do thay đổi:</strong> {changeReason}</p>
    </div>
    
    <p>Vui lòng đảm bảo bạn có mặt đúng giờ cho buổi dạy bù này.</p>
    <p>Nếu bạn có bất kì câu hỏi nào, vui lòng liên hệ với quản trị viên.</p>
    <p>Trân trọng,<br/>The InstruLearn Team</p>
</body>
</html>";

                            await _emailService.SendEmailAsync(teacherAccount.Email, subject, body);
                        }
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = isUpdating ?
                        "Makeup class updated successfully. Notifications sent to the learner and teacher." :
                        "Makeup class scheduled successfully. Notifications sent to the learner and teacher.",
                    Data = new
                    {
                        OriginalScheduleId = originalSchedule.ScheduleId,
                        MakeupScheduleId = makeupSchedule.ScheduleId,
                        IsUpdated = isUpdating,
                        OldDate = oldDate,
                        OldDayOfWeek = oldDayOfWeek.ToString(),
                        OldTimeStart = oldTimeStart.ToString("HH:mm"),
                        OldTimeEnd = oldTimeEnd.ToString("HH:mm"),
                        NewDate = newDate,
                        NewDayOfWeek = newDate.DayOfWeek.ToString(),
                        NewTimeStart = newTimeStart.ToString("HH:mm"),
                        NewTimeEnd = makeupSchedule.TimeEnd.ToString("HH:mm"),
                        TimeLearning = timeLearning,
                        ChangeReason = changeReason,
                        AttendanceStatus = makeupSchedule.AttendanceStatus.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to schedule makeup class: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> AutoUpdateAttendanceStatusAsync()
        {
            try
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                var overdueSchedules = await _unitOfWork.ScheduleRepository
                    .GetWhereAsync(s => s.AttendanceStatus == AttendanceStatus.NotYet &&
                                       s.StartDay < today);

                if (!overdueSchedules.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "No overdue schedules found that need updating.",
                        Data = new { UpdatedCount = 0 }
                    };
                }

                int updatedCount = 0;
                var notificationsSent = new List<object>();

                foreach (var schedule in overdueSchedules)
                {
                    schedule.AttendanceStatus = AttendanceStatus.Absent;
                    await _unitOfWork.ScheduleRepository.UpdateAsync(schedule);
                    updatedCount++;

                    if (schedule.LearnerId.HasValue)
                    {
                        var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(schedule.LearnerId.Value);

                        if (learner != null && learner.Account != null && !string.IsNullOrEmpty(learner.Account.Email))
                        {
                            string teacherName = "Your teacher";
                            if (schedule.TeacherId.HasValue)
                            {
                                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(schedule.TeacherId.Value);
                                teacherName = teacher?.Fullname ?? "Your teacher";
                            }

                            string formattedDate = schedule.StartDay.ToString("dd/MM/yyyy");
                            string formattedStartTime = schedule.TimeStart.ToString("HH:mm");
                            string formattedEndTime = schedule.TimeEnd.ToString("HH:mm");

                            string subject = "Missed Class Notification";
                            string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #f7f7f7; padding: 20px; border-radius: 5px;'>
                        <h2 style='color: #333;'>Thông báo vắng mặt</h2>
                        
                        <p>Xin chào {learner.FullName},</p>
                        
                        <p>Chúng tôi thấy rằng bạn đã bỏ lỡ buổi học lên lịch vào {formattedDate} từ {formattedStartTime} đến {formattedEndTime} với {teacherName}.</p>
                        
                        <div style='background-color: #f8d7da; padding: 15px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #dc3545;'>
                            <h3 style='margin-top: 0; color: #333;'>Chi tiết buổi học:</h3>
                            <p><strong>Ngày:</strong> {formattedDate}</p>
                            <p><strong>Thời gian:</strong> {formattedStartTime} - {formattedEndTime}</p>
                            <p><strong>Giáo viên:</strong> {teacherName}</p>
                            <p><strong>Trạng thái:</strong> Vắng mặt</p>
                        </div>
                        
                        <p>Nếu bạn muốn đặt lịch cho buổi học bù, vui lòng liên hệ với nhóm hỗ trợ của chúng tôi sớm nhất có thể.</p>
                        <p>Cảm ơn bạn đã sử dụng dịch vụ InstruLearn!</p>
                        <p>Trân trọng,<br/>The InstruLearn Team</p>
                    </div>
                </body>
                </html>";

                            await _emailService.SendEmailAsync(learner.Account.Email, subject, body);

                            notificationsSent.Add(new
                            {
                                ScheduleId = schedule.ScheduleId,
                                LearnerId = schedule.LearnerId.Value,
                                LearnerName = learner.FullName,
                                LearnerEmail = learner.Account.Email,
                                Date = schedule.StartDay.ToString("yyyy-MM-dd"),
                                Time = $"{schedule.TimeStart:HH:mm} - {schedule.TimeEnd:HH:mm}"
                            });
                        }
                    }
                }

                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Successfully updated {updatedCount} overdue schedules to Absent status. Sent {notificationsSent.Count} email notifications.",
                    Data = new
                    {
                        UpdatedCount = updatedCount,
                        NotificationsSent = notificationsSent
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to update attendance status automatically: {ex.Message}"
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
