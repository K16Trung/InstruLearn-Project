using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Payment;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IPaymentService
    {
        Task<ResponseDTO> ProcessLearningRegisPaymentAsync(CreatePaymentDTO paymentDTO);
        Task<ResponseDTO> ProcessRemainingPaymentAsync(int learningRegisId);
        Task<ResponseDTO> RejectPaymentAsync(int learningRegisId);
        Task<ResponseDTO> GetClassInitialPaymentsAsync(int? classId);

    }
}
