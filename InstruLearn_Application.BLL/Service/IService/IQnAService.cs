using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.QnA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IQnAService
    {
        Task<ResponseDTO> CreateQnAAsync(CreateQnADTO createQnADTO);
        Task<ResponseDTO> UpdateQnAAsync(int questionId, UpdateQnADTO updateQnADTO);
        Task<ResponseDTO> DeleteQnAAsync(int questionId);
        Task<QnADTO> GetQnAByIdAsync(int questionId);
        Task<List<QnADTO>> GetAllQnAAsync();
    }
}
