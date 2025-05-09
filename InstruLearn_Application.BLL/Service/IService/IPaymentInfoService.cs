using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IPaymentInfoService
    {
        Task<ResponseDTO> GetPaymentPeriodsInfoAsync(int learningRegisId);

        Task<ResponseDTO> EnrichLearningRegisWithPaymentInfoAsync(ResponseDTO learningRegisResponse);

        Task<ResponseDTO> EnrichSingleLearningRegisWithPaymentInfoAsync(int learningRegisId, ResponseDTO learningRegisResponse);
    }
}
