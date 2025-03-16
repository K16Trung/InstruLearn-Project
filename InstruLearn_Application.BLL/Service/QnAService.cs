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
                    Message = "Account not found",
                };
            }
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(createQnADTO.CoursePackageId);
            if (course == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Course not found",
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
                Message = "Question added successfully",
            };

            return response;
        }
        public async Task<ResponseDTO> UpdateQnAAsync(int id, UpdateQnADTO updateQnADTO)
        {
            var qnaUpdate = await _unitOfWork.QnARepository.GetByIdAsync(id);
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
                        Message = "Question update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Question update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Question not found!"
            };
        }
        public async Task<ResponseDTO> DeleteQnAAsync(int id)
        {
            var deleteQnA = await _unitOfWork.QnARepository.GetByIdAsync(id);
            if (deleteQnA != null)
            {
                await _unitOfWork.QnARepository.DeleteAsync(id);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Question deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Question with ID {id} not found"
                };
            }

        }
    }
}
