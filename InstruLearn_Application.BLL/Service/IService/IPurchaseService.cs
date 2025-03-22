using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IPurchaseService
    {
        Task<List<ResponseDTO>> GetAllPurchaseAsync();
        Task<ResponseDTO> GetPurchaseByIdAsync(int purchaseId);
        Task<ResponseDTO> GetPurchaseByLearnerIdAsync(int learnerId);
        Task<ResponseDTO> DeletePurchaseAsync(int purchaseId);
    }
}
