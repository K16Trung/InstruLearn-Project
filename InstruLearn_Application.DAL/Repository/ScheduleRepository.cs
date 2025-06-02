using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class ScheduleRepository : GenericRepository<Schedules>, IScheduleRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public ScheduleRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddRangeAsync(IEnumerable<Schedules> schedules)
        {
            await _appDbContext.Set<Schedules>().AddRangeAsync(schedules);
        }

        public async Task<List<Schedules>> GetSchedulesByLearningRegisIdAsync(int learningRegisId)
        {
            return await _appDbContext.Schedules
                .Include(s => s.ScheduleDays)
                .Include(s => s.Registration)
                .Where(s => s.LearningRegisId == learningRegisId)
                .ToListAsync();
        }

        public async Task<List<Schedules>> GetSchedulesByLearnerAsync(int learnerId)
        {
            return await _appDbContext.Schedules
                .Where(s => s.LearnerId == learnerId)
                .Include(s => s.ScheduleDays)
                .ThenInclude(sd => sd.DayOfWeeks)
                .ToListAsync();
        }

        public async Task<List<Schedules>> GetSchedulesByTeacherAsync(int teacherId)
        {
            return await _appDbContext.Schedules
                .Where(s => s.TeacherId == teacherId)
                .Include(s => s.ScheduleDays)
                .ThenInclude(sd => sd.DayOfWeeks)
                .Include(s => s.Learner)
                .ThenInclude(l => l.Account)
                .ToListAsync();
        }

        public async Task<List<ScheduleDTO>> GetSchedulesByLearnerIdAsync(int learnerId)
        {
            var schedules = await _appDbContext.Schedules
                .Include(s => s.ScheduleDays)
                .Where(s => s.LearnerId == learnerId)
                .Select(s => new
                {
                    s.ScheduleId,
                    s.TeacherId,
                    s.LearnerId,
                    s.TimeStart,
                    s.TimeEnd,
                    s.Mode,
                    s.LearningRegisId,
                    s.Registration.StartDay,
                    ScheduleDays = s.ScheduleDays.Select(d => d.DayOfWeeks).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            var formattedSchedules = schedules.Select(s => new ScheduleDTO
            {
                ScheduleId = s.ScheduleId,
                TeacherId = s.TeacherId,
                LearnerId = s.LearnerId,
                TimeStart = s.TimeStart.ToString("HH:mm"),
                TimeEnd = s.TimeEnd.ToString("HH:mm"),
                Mode = s.Mode,
                LearningRegisId = s.LearningRegisId ?? 0,
                RegistrationStartDay = s.StartDay,
                ScheduleDays = s.ScheduleDays.Select(day => new ScheduleDaysDTO
                {
                    DayOfWeeks = day
                }).ToList()
            }).ToList();

            return formattedSchedules;
        }

        public async Task<List<Schedules>> GetClassSchedulesByTeacherIdAsync(int teacherId)
        {
            return await _appDbContext.Schedules
                .Where(s => s.TeacherId == teacherId && s.Mode == ScheduleMode.Center)
                .Include(s => s.Teacher)
                .Include(s => s.Learner)
                .ThenInclude(l => l.Account)
                .Include(s => s.Class)
                .Include(s => s.Registration)
                .Include(s => s.ScheduleDays)
                .ToListAsync();
        }
        
        public async Task<List<int>> GetFreeTeacherIdsAsync(int majorId, TimeOnly timeStart, int timeLearning, DateOnly[] startDay)
        {
            TimeOnly timeEnd = timeStart.AddMinutes(timeLearning);

            var activeTeacherIdsForMajor = await _appDbContext.TeacherMajors
                .Where(tm => tm.MajorId == majorId &&
                             tm.Status == TeacherMajorStatus.Free)
                .Select(tm => tm.TeacherId)
                .ToListAsync();

            if (!activeTeacherIdsForMajor.Any())
            {
                return new List<int>();
            }

            var busyTeacherIds = new HashSet<int>();

            foreach (var day in startDay)
            {
                var busyOnThisDay = await _appDbContext.Schedules
                    .Where(s => s.TeacherId.HasValue &&
                                s.StartDay == day &&
                                (
                                    (s.TimeStart <= timeStart && s.TimeEnd > timeStart) ||
                                    (s.TimeStart < timeEnd && s.TimeEnd >= timeEnd) ||
                                    (s.TimeStart >= timeStart && s.TimeEnd <= timeEnd) ||
                                    (timeStart <= s.TimeStart && timeEnd >= s.TimeEnd)
                                ))
                    .Select(s => s.TeacherId.Value)
                    .Distinct()
                    .ToListAsync();

                foreach (var teacherId in busyOnThisDay)
                {
                    busyTeacherIds.Add(teacherId);
                }
            }

            var freeTeacherIds = activeTeacherIdsForMajor
                .Where(teacherId => !busyTeacherIds.Contains(teacherId))
                .ToList();

            return freeTeacherIds;
        }

        public async Task<List<Schedules>> GetSchedulesByTeacherIdAsync(int teacherId)
        {
            return await _appDbContext.Schedules
                .Where(s => s.TeacherId == teacherId && s.Mode == ScheduleMode.OneOnOne)
                .ToListAsync();
        }

        public async Task<List<Schedules>> GetWhereAsync(Expression<Func<Schedules, bool>> predicate)
        {
            return await _appDbContext.Schedules.Where(predicate).ToListAsync();
        }

        public async Task<List<ConsolidatedScheduleDTO>> GetConsolidatedCenterSchedulesByTeacherIdAsync(int teacherId)
        {
            var schedules = await _appDbContext.Schedules
                .Where(s => s.TeacherId == teacherId && s.Mode == ScheduleMode.Center)
                .Include(s => s.Teacher)
                .Include(s => s.Class)
                .Include(s => s.Learner)
                .ToListAsync();

            if (schedules == null || !schedules.Any())
            {
                return new List<ConsolidatedScheduleDTO>();
            }

            var groupedSchedules = schedules
                .GroupBy(s => new
                {
                    s.ClassId,
                    s.StartDay,
                    s.TimeStart,
                    s.TimeEnd
                })
                .Select(group => new
                {
                    ClassId = group.Key.ClassId,
                    StartDay = group.Key.StartDay,
                    TimeStart = group.Key.TimeStart,
                    TimeEnd = group.Key.TimeEnd,
                    Schedules = group.ToList(),
                    FirstSchedule = group.First()
                })
                .ToList();

            var classIds = schedules
                .Where(s => s.ClassId.HasValue)
                .Select(s => s.ClassId.Value)
                .Distinct()
                .ToList();

            var classDays = await _appDbContext.ClassDays
                .Where(cd => classIds.Contains(cd.ClassId))
                .ToListAsync();

            var classDaysByClass = classDays
                .GroupBy(cd => cd.ClassId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var classes = await _appDbContext.Classes
                .Where(c => classIds.Contains(c.ClassId))
                .ToListAsync();

            var classDict = classes.ToDictionary(c => c.ClassId, c => c);

            var learnerIds = schedules
                .Where(s => s.LearnerId.HasValue)
                .Select(s => s.LearnerId.Value)
                .Distinct()
                .ToList();

            var learners = new Dictionary<int, Learner>();
            if (learnerIds.Any())
            {
                learners = await _appDbContext.Learners
                    .Where(l => learnerIds.Contains(l.LearnerId))
                    .ToDictionaryAsync(l => l.LearnerId, l => l);
            }

            var consolidatedSchedules = new List<ConsolidatedScheduleDTO>();

            foreach (var group in groupedSchedules)
            {
                string className = "N/A";
                if (group.ClassId.HasValue && classDict.TryGetValue(group.ClassId.Value, out var classEntity))
                {
                    className = classEntity.ClassName;
                }

                var scheduleParticipants = new List<ScheduleParticipantDTO>();

                var participantGroups = group.Schedules
                    .Where(s => s.LearnerId.HasValue && s.LearningRegisId.HasValue)
                    .GroupBy(s => new { s.LearnerId, s.LearningRegisId })
                    .ToList();

                foreach (var participantGroup in participantGroups)
                {
                    if (participantGroup.Key.LearnerId.HasValue &&
                        learners.TryGetValue(participantGroup.Key.LearnerId.Value, out var learner) &&
                        participantGroup.Key.LearningRegisId.HasValue)
                    {
                        var schedule = participantGroup.First();

                        scheduleParticipants.Add(new ScheduleParticipantDTO
                        {
                            ScheduleId = schedule.ScheduleId,
                            LearnerId = participantGroup.Key.LearnerId.Value,
                            LearnerName = learner.FullName,
                            LearningRegisId = participantGroup.Key.LearningRegisId.Value,
                            AttendanceStatus = schedule.AttendanceStatus
                        });
                    }
                }

                var scheduleDays = new List<ScheduleDaysDTO>();
                if (group.ClassId.HasValue &&
                    classDaysByClass.TryGetValue(group.ClassId.Value, out var days))
                {
                    scheduleDays = days.Select(cd => new ScheduleDaysDTO
                    {
                        DayOfWeeks = cd.Day
                    }).ToList();
                }

                var consolidatedSchedule = new ConsolidatedScheduleDTO
                {
                    ScheduleId = group.FirstSchedule.ScheduleId,
                    TeacherId = teacherId,
                    TeacherName = group.FirstSchedule.Teacher?.Fullname ?? "N/A",
                    ClassId = group.ClassId,
                    ClassName = className,
                    TimeStart = group.TimeStart.ToString("HH:mm"),
                    TimeEnd = group.TimeEnd.ToString("HH:mm"),
                    DayOfWeek = group.StartDay.DayOfWeek.ToString(),
                    StartDay = group.StartDay,
                    Mode = ScheduleMode.Center,
                    AttendanceStatus = group.FirstSchedule.AttendanceStatus,
                    RegistrationStartDay = group.StartDay,
                    Participants = scheduleParticipants,
                    ScheduleDays = scheduleDays
                };

                consolidatedSchedules.Add(consolidatedSchedule);
            }

            return consolidatedSchedules;
        }

        public async Task<List<Schedules>> GetClassSchedulesByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.Schedules
                .Where(s => s.LearnerId == learnerId && s.Mode == ScheduleMode.Center)
                .Include(s => s.Teacher)
                .Include(s => s.Class)
                    .ThenInclude(c => c.ClassDays)
                .Include(s => s.Learner)
                    .ThenInclude(l => l.Account)
                .Include(s => s.Registration)
                .Include(s => s.ScheduleDays)
                .ToListAsync();
        }

        public async Task<List<AttendanceDTO>> GetClassAttendanceAsync(int classId)
        {
            var schedules = await _appDbContext.Schedules
                .Where(s => s.ClassId == classId && s.Mode == ScheduleMode.Center)
                .Include(s => s.Learner)
                .ToListAsync();

            return schedules.Select(s => new AttendanceDTO
            {
                ScheduleId = s.ScheduleId,
                StartDay = s.StartDay,
                TimeStart = s.TimeStart,
                TimeEnd = s.TimeEnd,
                LearnerId = s.LearnerId ?? 0,
                LearnerName = s.Learner.FullName ?? "Unknown",
                AttendanceStatus = s.AttendanceStatus
            }).ToList();
        }

        public async Task<List<AttendanceDTO>> GetOneOnOneAttendanceAsync(int learnerId)
        {
            var schedules = await _appDbContext.Schedules
                .Where(s => s.LearnerId == learnerId && s.Mode == ScheduleMode.OneOnOne)
                .Include(s => s.Learner)
                .ToListAsync();

            return schedules.Select(s => new AttendanceDTO
            {
                ScheduleId = s.ScheduleId,
                StartDay = s.StartDay,
                TimeStart = s.TimeStart,
                TimeEnd = s.TimeEnd,
                LearnerId = s.LearnerId ?? 0,
                LearnerName = s.Learner.FullName ?? "Unknown",
                AttendanceStatus = s.AttendanceStatus
            }).ToList();
        }

        public async Task<(bool HasConflict, List<Schedules> ConflictingSchedules)> CheckLearnerScheduleConflictAsync(
    int learnerId, DateOnly startDay, TimeOnly timeStart, int durationMinutes)
        {
            TimeOnly timeEnd = timeStart.AddMinutes(durationMinutes);

            var conflictingSchedules = await _appDbContext.Schedules
                .Where(s => s.LearnerId == learnerId &&
                           s.StartDay == startDay &&
                           (
                               (timeStart >= s.TimeStart && timeStart < s.TimeEnd) ||
                               (timeEnd > s.TimeStart && timeEnd <= s.TimeEnd) ||
                               (s.TimeStart >= timeStart && s.TimeStart < timeEnd) ||
                               (s.TimeEnd > timeStart && s.TimeEnd <= timeEnd)
                           ))
                .Include(s => s.Class)
                .Include(s => s.Teacher)
                .Include(s => s.Registration)
                .ToListAsync();

            return (conflictingSchedules.Any(), conflictingSchedules);
        }

        public async Task<(bool HasConflict, List<Schedules> ConflictingSchedules)> CheckLearnerClassScheduleConflictAsync(
    int learnerId, int classId)
        {
            var learnerSchedules = await _appDbContext.Schedules
                .Where(s => s.LearnerId == learnerId)
                .Include(s => s.Class)
                .Include(s => s.Teacher)
                .ToListAsync();

            var classEntity = await _appDbContext.Classes
                .Include(c => c.ClassDays)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classEntity == null)
            {
                return (false, new List<Schedules>());
            }

            TimeOnly classTimeStart = classEntity.ClassTime;
            TimeOnly classTimeEnd = classEntity.ClassTime.AddHours(2);

            var classDayValues = classEntity.ClassDays.Select(cd => (int)cd.Day).ToList();

            var conflictingSchedules = learnerSchedules
                .Where(s => classDayValues.Contains((int)s.StartDay.DayOfWeek) &&
                            (classTimeStart < s.TimeEnd && s.TimeStart < classTimeEnd))
                .ToList();

            var pendingRegistrations = await _appDbContext.Learning_Registrations
                .Where(r => r.LearnerId == learnerId &&
                           (r.Status == LearningRegis.Pending ||
                            r.Status == LearningRegis.Accepted))
                .Include(r => r.LearningRegistrationDay)
                .ToListAsync();

            foreach (var registration in pendingRegistrations)
            {
                if (!registration.StartDay.HasValue) continue;

                var registrationDayValues = registration.LearningRegistrationDay
                    .Select(ld => (int)ld.DayOfWeek)
                    .ToList();

                bool hasDayOverlap = registrationDayValues.Any(regDay => classDayValues.Contains(regDay));

                if (hasDayOverlap)
                {
                    TimeOnly regisStart = registration.TimeStart;
                    TimeOnly regisEnd = registration.TimeStart.AddMinutes(registration.TimeLearning);

                    bool hasTimeOverlap = (classTimeStart < regisEnd && regisStart < classTimeEnd);

                    if (hasTimeOverlap)
                    {
                        var virtualSchedule = new Schedules
                        {
                            ScheduleId = -1, // Virtual ID
                            LearnerId = registration.LearnerId,
                            StartDay = registration.StartDay.Value,
                            TimeStart = registration.TimeStart,
                            TimeEnd = regisEnd,
                            Registration = registration
                        };

                        conflictingSchedules.Add(virtualSchedule);
                    }
                }
            }

            return (conflictingSchedules.Any(), conflictingSchedules);
        }
    }
}
