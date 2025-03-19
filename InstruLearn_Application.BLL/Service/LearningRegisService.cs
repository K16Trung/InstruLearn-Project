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
using InstruLearn_Application.Model.Enum;

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
            // Check if the learner has a wallet
            var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == createLearningRegisDTO.LearnerId);

            if (wallet == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Wallet not found for the learner.",
                };
            }

            // Check if the balance is sufficient
            if (wallet.Balance < 50000)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Insufficient balance in the wallet.",
                };
            }

            wallet.Balance -= 50000;
            _unitOfWork.WalletRepository.UpdateAsync(wallet);

            var learningRegis = _mapper.Map<Learning_Registration>(createLearningRegisDTO);
            learningRegis.Status = LearningRegis.Pending; 

            await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);

            await _unitOfWork.SaveChangeAsync();

            // Add LearningRegistrationDay records
            if (createLearningRegisDTO.LearningDays != null && createLearningRegisDTO.LearningDays.Any())
            {
                var learningDays = createLearningRegisDTO.LearningDays.Select(day => new LearningRegistrationDay
                {
                    LearningRegisId = learningRegis.LearningRegisId,  // Ensure FK is set
                    DayOfWeek = day
                }).ToList();

                await _unitOfWork.LearningRegisDayRepository.AddRangeAsync(learningDays);
            }

            // Create a wallet transaction for this deduction
            var walletTransaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid().ToString(), // Generate unique transaction ID
                WalletId = wallet.WalletId,
                Amount = 50000,
                TransactionType = TransactionType.Payment,
                Status = TransactionStatus.Complete,
                TransactionDate = DateTime.UtcNow
            };

            await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);

            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration added successfully. Wallet balance updated. Status set to Pending.",
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

        public async Task<ResponseDTO> GetAllPendingRegistrationsAsync()
        {
            var pendingRegistrations = await _learningRegisRepository.GetPendingRegistrationsAsync();
            var pendingDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(pendingRegistrations);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Pending learning registrations retrieved successfully.",
                Data = pendingDtos
            };
        }

        public async Task<ResponseDTO> GetPendingRegistrationsByLearnerIdAsync(int learnerId)
        {
            var pendingRegistrations = await _learningRegisRepository.GetPendingRegistrationsByLearnerIdAsync(learnerId);
            var pendingDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(pendingRegistrations);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = $"Pending registrations for Learner ID {learnerId} retrieved successfully.",
                Data = pendingDtos
            };
        }

        public async Task<ResponseDTO> UpdateLearningRegisStatusAsync(UpdateLearningRegisDTO updateDTO)
        {
            var learningRegis = await _unitOfWork.LearningRegisRepository.GetByIdAsync(updateDTO.LearningRegisId);

            if (learningRegis == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning Registration not found."
                };
            }

            _mapper.Map(updateDTO, learningRegis);

            learningRegis.Status = LearningRegis.Accepted;

            _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration updated successfully. Status changed to Accepted."
            };
        }
    }
}
