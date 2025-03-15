using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningRegisService : ILearningRegisService
    {
        private readonly ILearningRegisRepository _learningRegisRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LearningRegisService(ILearningRegisRepository learningRegisRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _learningRegisRepository = learningRegisRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> GetAllLearningRegisAsync()
        {
            var learningRegis = await _unitOfWork.LearningRegisRepository.GetAllAsync();
            var learningRegisDtos = _mapper.Map<IEnumerable<LearningRegisDTO>>(learningRegis);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration retrieved successfully.",
                Data = learningRegisDtos
            };
        }

        public async Task<ResponseDTO> GetLearningRegisByIdAsync(int learningRegisId)
        {
            var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
            if (learningRegis == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning Registration not found."
                };
            }
            var learningRegisDtos = _mapper.Map<LearningRegisDTO>(learningRegisId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration retrieved successfully.",
                Data = learningRegisDtos
            };
        }

        public async Task<ResponseDTO> CreateLearningRegisAsync(CreateLearningRegisDTO createLearningRegisDTO)
        {
            var learningRegis = _mapper.Map<Learning_Registration>(createLearningRegisDTO);
            await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration added successfully.",
            };
        }

        public async Task<ResponseDTO> DeleteLearningRegisAsync(int learningRegisId)
        {
            var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(learningRegisId);
            if (learningRegis == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning Registration not found."
                };
            }
            await _unitOfWork.LearningRegisRepository.DeleteAsync(learningRegisId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration deleted successfully."
            };
        }
    }
}
