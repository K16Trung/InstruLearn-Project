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
    }
}
