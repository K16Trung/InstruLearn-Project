using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.QnAReplies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IQnARepliesService
    {
        Task<ResponseDTO> CreateQnARepliesAsync(CreateQnARepliesDTO createQnARepliesDTO);
        Task<ResponseDTO> UpdateQnARepliesAsync(int replyId, UpdateQnARepliesDTO updateQnARepliesDTO);
        Task<ResponseDTO> DeleteQnARepliesAsync(int replyId);
        Task<ResponseDTO> GetQnARepliesByIdAsync(int replyId);
        Task<ResponseDTO> GetAllQnARepliesAsync();
    }
}
