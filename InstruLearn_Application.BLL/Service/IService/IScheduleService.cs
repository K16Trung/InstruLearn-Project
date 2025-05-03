using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.Schedules;
using InstruLearn_Application.Model.Models.DTO.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IScheduleService
    {
        Task<List<ScheduleDTO>> GetSchedulesByLearningRegisIdAsync(int learningRegisId);
        Task<ResponseDTO> GetSchedulesAsync(int learningRegisId);
        Task<ResponseDTO> GetSchedulesByLearnerIdAsync(int learnerId);
        Task<ResponseDTO> GetSchedulesByTeacherIdAsync(int teacherId);
        Task<ResponseDTO> GetClassSchedulesByTeacherIdAsync(int teacherId);
        Task<ResponseDTO> GetClassSchedulesByTeacherIdAsyncc(int teacherId);
        Task<List<ValidTeacherDTO>> GetAvailableTeachersAsync(int majorId, TimeOnly timeStart, int timeLearning, DateOnly startDay);
        Task<ResponseDTO> GetClassSchedulesByLearnerIdAsync(int learnerId);
        Task<ResponseDTO> GetClassAttendanceAsync(int classId);
        Task<ResponseDTO> GetOneOnOneAttendanceAsync(int learnerId);
        Task<ResponseDTO> UpdateAttendanceAsync(int scheduleId, AttendanceStatus status);
        Task<ResponseDTO> CheckLearnerScheduleConflictAsync(int learnerId, DateOnly startDay, TimeOnly timeStart, int durationMinutes);
        Task<ResponseDTO> CheckLearnerClassScheduleConflictAsync(int learnerId, int classId);
        Task<ResponseDTO> GetLearnerAttendanceStatsAsync(int learnerId);
        Task<ResponseDTO> UpdateScheduleTeacherAsync(int scheduleId, int teacherId, string changeReason);
        Task<ResponseDTO> UpdateScheduleForMakeupAsync(int scheduleId, DateOnly newDate, TimeOnly newTimeStart, int timeLearning, string changeReason);
        Task<ResponseDTO> AutoUpdateAttendanceStatusAsync();

    }
}
