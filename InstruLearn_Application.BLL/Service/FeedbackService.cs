using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<FeedbackDTO>> GetAllFeedbackAsync()
        {
            var feedbackGetAll = await _unitOfWork.FeedbackRepository.GetAllAsync();
            var feedbackMapper = _mapper.Map<List<FeedbackDTO>>(feedbackGetAll);
            return feedbackMapper;
        }
        public async Task<FeedbackDTO> GetFeedbackByIdAsync(int feedbackId)
        {
            var feedbackGetById = await _unitOfWork.FeedbackRepository.GetByIdAsync(feedbackId);
            var feedbackMapper = _mapper.Map<FeedbackDTO>(feedbackGetById);
            return feedbackMapper;
        }
        public async Task<ResponseDTO> CreateFeedbackAsync(CreateFeedbackDTO createfeedbackDTO)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(createfeedbackDTO.AccountId);
            if (account == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Account not found",
                };
            }
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(createfeedbackDTO.CourseId);
            if (course == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course not found",
                };
            }

            var feedbackObj = _mapper.Map<FeedBack>(createfeedbackDTO);
            feedbackObj.Account = account;
            feedbackObj.CoursePackage = course;
            feedbackObj.CreateAt = DateTime.Now;

            await _unitOfWork.FeedbackRepository.AddAsync(feedbackObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback added successfully",
            };

            return response;
        }
        public async Task<ResponseDTO> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackDTO updatefeedbackDTO)
        {
            var feedbackUpdate = await _unitOfWork.FeedbackRepository.GetByIdAsync(feedbackId);
            if (feedbackUpdate != null)
            {
                feedbackUpdate = _mapper.Map(updatefeedbackDTO, feedbackUpdate);
                await _unitOfWork.FeedbackRepository.UpdateAsync(feedbackUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Feedback update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Feedback update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Feedback not found!"
            };
        }
        public async Task<ResponseDTO> DeleteFeedbackAsync(int feedbackId)
        {
            var deleteFeedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(feedbackId);
            if (deleteFeedback != null)
            {
                await _unitOfWork.FeedbackRepository.DeleteAsync(feedbackId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Feedback deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Feedback with ID {feedbackId} not found"
                };
            }

        }
    }
}
