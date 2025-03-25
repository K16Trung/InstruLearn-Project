using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.FeedbackReplies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class FeedbackRepliesService : IFeedbackRepliesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FeedbackRepliesService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<FeedbackRepliesDTO>> GetAllFeedbackRepliesAsync()
        {
            var feedbackRepliesGetAll = await _unitOfWork.FeedbackRepliesRepository.GetAllAsync();
            var feedbackRepliesMapper = _mapper.Map<List<FeedbackRepliesDTO>>(feedbackRepliesGetAll);
            return feedbackRepliesMapper;
        }
        public async Task<FeedbackRepliesDTO> GetFeedbackRepliesByIdAsync(int feedbackRepliesId)
        {
            var feedbackRepliesGetById = await _unitOfWork.FeedbackRepliesRepository.GetByIdAsync(feedbackRepliesId);
            var feedbackRepliesMapper = _mapper.Map<FeedbackRepliesDTO>(feedbackRepliesGetById);
            return feedbackRepliesMapper;
        }
        public async Task<ResponseDTO> CreateFeedbackRepliesAsync(CreateFeedbackRepliesDTO createfeedbackrepliesDTO)
        {
            var feedbackreply = await _unitOfWork.FeedbackRepository.GetByIdAsync(createfeedbackrepliesDTO.FeedbackId);
            if (feedbackreply == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy nhận xét",
                };
            }
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(createfeedbackrepliesDTO.AccountId);
            if (account == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy tài khoản",
                };
            }
            var feedbackreplyObj = _mapper.Map<FeedbackReplies>(createfeedbackrepliesDTO);

            feedbackreplyObj.Account = account;

            feedbackreplyObj.CreateAt = DateTime.Now;

            await _unitOfWork.FeedbackRepliesRepository.AddAsync(feedbackreplyObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Thêm phản hồi thành công!",
            };
            return response;
        }
        public async Task<ResponseDTO> UpdateFeedbackRepliesAsync(int replyId, UpdateFeedbackRepliesDTO updatefeedbackrepliesDTO)
        {
            var feedbackrepliesUpdate = await _unitOfWork.FeedbackRepliesRepository.GetByIdAsync(replyId);
            if (feedbackrepliesUpdate != null)
            {
                feedbackrepliesUpdate = _mapper.Map(updatefeedbackrepliesDTO, feedbackrepliesUpdate);
                await _unitOfWork.FeedbackRepliesRepository.UpdateAsync(feedbackrepliesUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Cập nhật phản hồi thành công!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Cập nhật phản hồi thất bại!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Không tìm thấy phản hồi!"
            };
        }
        public async Task<ResponseDTO> DeleteFeedbackRepliesAsync(int replyId)
        {
            var deleteFeedbackreplies = await _unitOfWork.FeedbackRepliesRepository.GetByIdAsync(replyId);
            if (deleteFeedbackreplies != null)
            {
                await _unitOfWork.FeedbackRepliesRepository.DeleteAsync(replyId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Xóa phản hồi thành công"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy phản hồi với ID {replyId}"
                };
            }

        }
    }
}
