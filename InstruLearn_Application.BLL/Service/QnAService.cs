using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.QnA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class QnAService : IQnAService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QnAService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<QnADTO>> GetAllQnAAsync()
        {
            var qnaGetAll = await _unitOfWork.QnARepository.GetAllAsync();
            var qnaMapper = _mapper.Map<List<QnADTO>>(qnaGetAll);
            return qnaMapper;
        }
        public async Task<QnADTO> GetQnAByIdAsync(int questionId)
        {
            var qnaGetById = await _unitOfWork.QnARepository.GetByIdAsync(questionId);
            var qnaMapper = _mapper.Map<QnADTO>(qnaGetById);
            return qnaMapper;
        }
        public async Task<ResponseDTO> CreateQnAAsync(CreateQnADTO createQnADTO)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(createQnADTO.AccountId);
            if (account == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy tài khoản",
                };
            }
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(createQnADTO.CoursePackageId);
            if (course == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy gói học",
                };
            }

            var qnaObj = _mapper.Map<QnA>(createQnADTO);
            qnaObj.Account = account;
            qnaObj.CoursePackage = course;
            qnaObj.CreateAt = DateTime.Now;

            await _unitOfWork.QnARepository.AddAsync(qnaObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Câu hỏi đã được thêm thành công",
            };

            return response;
        }
        public async Task<ResponseDTO> UpdateQnAAsync(int questionId, UpdateQnADTO updateQnADTO)
        {
            var qnaUpdate = await _unitOfWork.QnARepository.GetByIdAsync(questionId);
            if (qnaUpdate != null)
            {
                qnaUpdate = _mapper.Map(updateQnADTO, qnaUpdate);
                await _unitOfWork.QnARepository.UpdateAsync(qnaUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Đã cập nhật câu hỏi thành công!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Đã cập nhật câu hỏi thất bại!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Không tìm thấy câu hỏi!"
            };
        }
        public async Task<ResponseDTO> DeleteQnAAsync(int questionId)
        {
            var deleteQnA = await _unitOfWork.QnARepository.GetByIdAsync(questionId);
            if (deleteQnA != null)
            {
                await _unitOfWork.QnARepository.DeleteAsync(questionId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Câu hỏi đã được xóa thành công"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy câu hỏi có ID {questionId}"
                };
            }

        }
    }
}
