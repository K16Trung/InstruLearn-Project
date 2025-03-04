using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.ClassDay;

namespace InstruLearn_Application.BLL.Service
{
    public class ClassDayService : IClassDayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassDayService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<ClassDayDTO>> GetAllClassDayAsync()
        {
            var classDayGetAll = await _unitOfWork.ClassDayRepository.GetAllAsync();
            var classDayMapper = _mapper.Map<List<ClassDayDTO>>(classDayGetAll);
            return classDayMapper;
        }
        public async Task<ClassDayDTO> GetFeedbackByIdAsync(int classDayId)
        {
            var classDayGetById = await _unitOfWork.ClassDayRepository.GetByIdAsync(classDayId);
            var classDayMapper = _mapper.Map<ClassDayDTO>(classDayGetById);
            return classDayMapper;
        }
        public async Task<ResponseDTO> CreateClassDayAsync(CreateClassDayDTO createClassDayDTO)
        {
            var classfind = await _unitOfWork.AccountRepository.GetByIdAsync(createClassDayDTO.ClassId);
            if (classfind == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Class not found",
                };
            }

            var classObj = _mapper.Map<ClassDay>(createClassDayDTO);
            classObj.Class = classfind;

            await _unitOfWork.ClassDayRepository.AddAsync(classObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Feedback added successfully",
            };

            return response;
        }
        public async Task<ResponseDTO> UpdateFeedbackAsync(int id, UpdateFeedbackDTO updatefeedbackDTO)
        {
            var feedbackUpdate = await _unitOfWork.FeedbackRepository.GetByIdAsync(id);
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
        public async Task<ResponseDTO> DeleteFeedbackAsync(int id)
        {
            var deleteFeedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(id);
            if (deleteFeedback != null)
            {
                await _unitOfWork.FeedbackRepository.DeleteAsync(id);
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
                    Message = $"Feedback with ID {id} not found"
                };
            }
        }
}
