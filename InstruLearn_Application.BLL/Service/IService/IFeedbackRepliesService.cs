using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.FeedbackReplies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IFeedbackRepliesService
    {
        Task<ResponseDTO> CreateFeedbackRepliesAsync(CreateFeedbackRepliesDTO createFeedbackRepliesDTO);
        Task<ResponseDTO> UpdateFeedbackRepliesAsync(int replyId, UpdateFeedbackRepliesDTO updateFeedbackRepliesDTO);
        Task<ResponseDTO> DeleteFeedbackRepliesAsync(int replyId);
        Task<FeedbackRepliesDTO> GetFeedbackRepliesByIdAsync(int replyId);
        Task<List<FeedbackRepliesDTO>> GetAllFeedbackRepliesAsync();
    }
}
