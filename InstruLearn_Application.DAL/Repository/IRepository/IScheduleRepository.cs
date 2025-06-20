﻿using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IScheduleRepository : IGenericRepository<Schedules>
    {
        Task AddRangeAsync(IEnumerable<Schedules> schedules);
        Task<List<Schedules>> GetSchedulesByLearningRegisIdAsync(int learningRegisId);
        Task<List<Schedules>> GetSchedulesByLearnerAsync(int learnerId);
        Task<List<Schedules>> GetSchedulesByTeacherAsync(int teacherId);
        Task<List<Schedules>> GetClassSchedulesByTeacherIdAsync(int teacherId);
        Task<List<ScheduleDTO>> GetSchedulesByLearnerIdAsync(int learnerId);
        Task<List<int>> GetFreeTeacherIdsAsync(int majorId, TimeOnly timeStart, int timeLearning, DateOnly[] startDay);
        Task<List<Schedules>> GetSchedulesByTeacherIdAsync(int teacherId);
        Task<List<Schedules>> GetWhereAsync(Expression<Func<Schedules, bool>> predicate);
        Task<List<ConsolidatedScheduleDTO>> GetConsolidatedCenterSchedulesByTeacherIdAsync(int teacherId);
        Task<List<Schedules>> GetClassSchedulesByLearnerIdAsync(int learnerId);
        Task<List<AttendanceDTO>> GetClassAttendanceAsync(int classId);
        Task<List<AttendanceDTO>> GetOneOnOneAttendanceAsync(int learnerId);
        Task<(bool HasConflict, List<Schedules> ConflictingSchedules)> CheckLearnerScheduleConflictAsync(
            int learnerId, DateOnly startDay, TimeOnly timeStart, int durationMinutes);
        Task<(bool HasConflict, List<Schedules> ConflictingSchedules)> CheckLearnerClassScheduleConflictAsync(
            int learnerId, int classId);

    }
}
