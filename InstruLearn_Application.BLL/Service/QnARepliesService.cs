using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.QnA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.QnAReplies;

namespace InstruLearn_Application.BLL.Service
{
    public class QnARepliesService : IQnARepliesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QnARepliesService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<QnARepliesDTO>> GetAllQnARepliesAsync()
        {
            var replyGetAll = await _unitOfWork.QnARepliesRepository.GetAllAsync();
            var replyMapper = _mapper.Map<List<QnARepliesDTO>>(replyGetAll);
            return replyMapper;
        }
        public async Task<QnARepliesDTO> GetQnARepliesByIdAsync(int replyId)
        {
            var replyGetById = await _unitOfWork.QnARepliesRepository.GetByIdAsync(replyId);
            var replyMapper = _mapper.Map<QnARepliesDTO>(replyGetById);
            return replyMapper;
        }
        public async Task<ResponseDTO> CreateQnARepliesAsync(CreateQnARepliesDTO createQnARepliesDTO)
        {
            var question = await _unitOfWork.QnARepository.GetByIdAsync(createQnARepliesDTO.QuestionId);
            if (question == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Question not found",
                };
            }
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(createQnARepliesDTO.AccountId);
            if (account == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Account not found",
                };
            }

            var replyObj = _mapper.Map<QnAReplies>(createQnARepliesDTO);
            replyObj.Account = account;
            replyObj.QnA = question;
            replyObj.CreateAt = DateTime.Now;

            await _unitOfWork.QnARepliesRepository.AddAsync(replyObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Reply added successfully",
            };

            return response;
        }
        public async Task<ResponseDTO> UpdateQnARepliesAsync(int id, UpdateQnARepliesDTO updateQnARepliesDTO)
        {
            var replyUpdate = await _unitOfWork.QnARepliesRepository.GetByIdAsync(id);
            if (replyUpdate != null)
            {
                replyUpdate = _mapper.Map(updateQnARepliesDTO, replyUpdate);
                await _unitOfWork.QnARepliesRepository.UpdateAsync(replyUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Reply update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Reply update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Reply not found!"
            };
        }
        public async Task<ResponseDTO> DeleteQnARepliesAsync(int id)
        {
            var deleteReply = await _unitOfWork.QnARepliesRepository.GetByIdAsync(id);
            if (deleteReply != null)
            {
                await _unitOfWork.QnARepliesRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Reply deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Reply with ID {id} not found"
                };
            }

        }
    }
}
