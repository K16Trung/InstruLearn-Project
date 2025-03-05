using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models.DTO.Learner;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;

namespace InstruLearn_Application.BLL.Service
{
    public class LearnerService : ILearnerService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtHelper _jwtHelper;

        public LearnerService(IUnitOfWork unitOfWork, IMapper mapper, JwtHelper jwtHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }
        public async Task<ResponseDTO> GetAllLearnerAsync()
        {
            var response = new ResponseDTO();

            var learners = await _unitOfWork.LearnerRepository
                .GetQuery()
                .Include(t => t.Account)
                .ToListAsync();

            var learnerDTOs = _mapper.Map<IEnumerable<LearnerDTO>>(learners);

            response.IsSucceed = true;
            response.Message = "Data retrieved successfully";
            response.Data = learnerDTOs;
            return response;
        }
        public async Task<ResponseDTO> DeleteLearnerAsync(int learnerId)
        {
            var response = new ResponseDTO();

            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
            if (learner == null)
            {
                response.Message = "Learner not found.";
                return response;
            }

            // Change status to Banned
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
            account.IsActive = AccountStatus.Banned;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Failed to delete learner.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Learner delete successfully!";
            return response;
        }

        public async Task<ResponseDTO> UnbanLearnerAsync(int learnerId)
        {
            var response = new ResponseDTO();

            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
            if (learner == null)
            {
                response.Message = "Learner not found.";
                return response;
            }

            // Change status to Banned
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(learner.AccountId);
            account.IsActive = AccountStatus.Active;

            var updated = await _unitOfWork.AccountRepository.UpdateAsync(account);

            if (!updated)
            {
                response.Message = "Failed to unban learner.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Learner unban successfully!";
            return response;
        }

        public async Task<ResponseDTO> GetLearnerByIdAsync(int learnerId)
        {
            var response = new ResponseDTO();
            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
            if (learner == null)
            {
                response.Message = "Learner not found.";
                return response;
            }
            var learnerDTO = _mapper.Map<LearnerDTO>(learner);
            response.IsSucceed = true;
            response.Message = "Learner retrieved successfully";
            response.Data = learnerDTO;
            return response;
        }

        public async Task<ResponseDTO> UpdateLearnerAsync(int learnerId, UpdateLearnerDTO updateLearnerDTO)
        {
            var response = new ResponseDTO();

            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(learnerId);
            if (learner == null)
            {
                response.Message = "Learner not found.";
                return response;
            }

            _mapper.Map(updateLearnerDTO, learner);

            var updated = await _unitOfWork.LearnerRepository.UpdateAsync(learner);

            if (!updated)
            {
                response.Message = "Failed to update learner.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Learner updated successfully!";
            return response;
        }
    }
}
