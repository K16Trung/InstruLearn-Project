using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.PurchaseItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IPurchaseItemService
    {
        Task<List<ResponseDTO>> GetAllPurchaseItemAsync();
        Task<ResponseDTO> GetPurchaseItemByIdAsync(int purchaseItemId);
        Task<ResponseDTO> GetPurchaseItemByLearnerIdAsync(int learnerId);
        Task<ResponseDTO> CreatePurchaseItemAsync(CreatePurchaseItemDTO purchaseItemDTO);
        Task<ResponseDTO> DeletePurchaseItemAsync(int purchaseItemId);
    }
}
