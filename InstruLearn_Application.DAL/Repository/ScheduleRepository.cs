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

        // Get schedules with Learning_Registration details
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
                .Include(s => s.ScheduleDays) // Include ScheduleDays
                .ThenInclude(sd => sd.DayOfWeeks) // Include DayOfWeeks for each day
                .ToListAsync();
        }

        public async Task<List<Schedules>> GetSchedulesByTeacherAsync(int teacherId)
        {
            return await _appDbContext.Schedules
                .Where(s => s.TeacherId == teacherId)
                .Include(s => s.ScheduleDays)
                .ThenInclude(sd => sd.DayOfWeeks)
                .ToListAsync();
        }

        public async Task<List<ScheduleDTO>> GetSchedulesByLearnerIdAsync(int learnerId)
        {
            var schedules = await _appDbContext.Schedules
                .Include(s => s.ScheduleDays)
                .Where(s => s.LearnerId == learnerId)  // Ensures filtering by learnerId
                .Select(s => new
                {
                    s.ScheduleId,
                    s.TeacherId,
                    s.LearnerId,
                    s.TimeStart,
                    s.TimeEnd,
                    s.Mode,
                    s.LearningRegisId,
                    s.Registration.StartDay,  // Nullable DateOnly
                    ScheduleDays = s.ScheduleDays.Select(d => d.DayOfWeeks).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            // Map to DTOs
            var formattedSchedules = schedules.Select(s => new ScheduleDTO
            {
                ScheduleId = s.ScheduleId,
                TeacherId = s.TeacherId,
                LearnerId = s.LearnerId,
                TimeStart = s.TimeStart.ToString("HH:mm"),
                TimeEnd = s.TimeEnd.ToString("HH:mm"),
                Mode = s.Mode,
                LearningRegisId = s.LearningRegisId ?? 0,
                RegistrationStartDay = s.StartDay,  // Pass DateOnly?
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
                .Include(s => s.Class)
                .Include(s => s.Registration)
                .Include(s => s.ScheduleDays)
                .ToListAsync();
        }

        public async Task<List<int>> GetFreeTeacherIdsAsync(int majorId, TimeOnly timeStart, int timeLearning, DateOnly startDay)
        {
            TimeOnly timeEnd = timeStart.AddMinutes(timeLearning);

            // Get day of week if needed for recurring schedules
            DayOfWeek dayOfWeek = startDay.DayOfWeek;

            var busyTeacherIds = await _appDbContext.Schedules
                .Where(s => s.TeacherId.HasValue &&
                            s.StartDay == startDay &&  // Direct date match
                            (
                                (s.TimeStart <= timeStart && s.TimeEnd > timeStart) ||  // Overlapping start
                                (s.TimeStart < timeEnd && s.TimeEnd >= timeEnd) ||      // Overlapping end
                                (s.TimeStart >= timeStart && s.TimeEnd <= timeEnd) ||   // Fully inside
                                (timeStart <= s.TimeStart && timeEnd >= s.TimeEnd)      // Completely contains
                            ))
                .Select(s => s.TeacherId.Value)
                .Distinct()
                .ToListAsync();

            // Get teachers who have an active relationship with the specified major
            var activeTeacherIdsForMajor = await _appDbContext.TeacherMajors
                .Where(tm => tm.MajorId == majorId &&
                             tm.Status == TeacherMajorStatus.Free)  // Assuming 1 is Active
                .Select(tm => tm.TeacherId)
                .ToListAsync();

            // Exclude busy teachers from the active teachers
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

    }
}
