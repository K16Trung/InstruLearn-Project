using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ILearnerNotificationService
    {
        Task<ResponseDTO> GetLearnerEmailNotificationsAsync(int learnerId);
        Task<ResponseDTO> GetEntranceTestNotificationsAsync(int learnerId, int? classId = null);
    }
}
