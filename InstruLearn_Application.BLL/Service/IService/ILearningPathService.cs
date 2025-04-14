using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ILearningPathService
    {
        Task<ResponseDTO> GetLearningPathSessionsAsync(int learningRegisId);
        Task<ResponseDTO> UpdateSessionCompletionStatusAsync(int learningPathSessionId, bool isCompleted);
    }
}
