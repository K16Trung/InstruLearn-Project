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
using System.Transactions;
using Microsoft.Extensions.Logging;
using InstruLearn_Application.Model.Models.DTO.Syllabus;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningRegisService : ILearningRegisService
    {
        private readonly ILearningRegisRepository _learningRegisRepository;
        private readonly ILogger<LearningRegisService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LearningRegisService(ILearningRegisRepository learningRegisRepository, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LearningRegisService> logger)
        {
            _learningRegisRepository = learningRegisRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<List<ResponseDTO>> GetAllLearningRegisAsync()
        {
            var learningRegisList = await _unitOfWork.LearningRegisRepository.GetAllAsync();
            var learningRegisDtos = _mapper.Map<IEnumerable<SyllabusDTO>>(learningRegisList);

            var responseList = new List<ResponseDTO>();

            foreach (var learningRegisDto in learningRegisDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Syllabus retrieved successfully.",
                    Data = learningRegisDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> GetLearningRegisByIdAsync(int learningRegisId)
        {
            var registration = await _learningRegisRepository.GetByIdAsync(learningRegisId);
            if (registration == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning registration not found.",
                    Data = null
                };
            }

            var dto = _mapper.Map<OneOnOneRegisDTO>(registration);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning registration retrieved successfully.",
                Data = dto
            };
        }

        public async Task<ResponseDTO> CreateLearningRegisAsync(CreateLearningRegisDTO createLearningRegisDTO)
        {
            try
            {
                _logger.LogInformation("Starting learning registration process.");

                // Start EF Core transaction instead of TransactionScope
                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    var wallet = await _unitOfWork.WalletRepository.GetFirstOrDefaultAsync(w => w.LearnerId == createLearningRegisDTO.LearnerId);

                    if (wallet == null)
                    {
                        _logger.LogWarning($"Wallet not found for learnerId: {createLearningRegisDTO.LearnerId}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Wallet not found for the learner."
                        };
                    }

                    _logger.LogInformation($"Wallet found for learnerId: {createLearningRegisDTO.LearnerId}, balance: {wallet.Balance}");

                    if (wallet.Balance < 50000)
                    {
                        _logger.LogWarning($"Insufficient balance for learnerId: {createLearningRegisDTO.LearnerId}. Current balance: {wallet.Balance}");
                        return new ResponseDTO
                        {
                            IsSucceed = false,
                            Message = "Insufficient balance in the wallet."
                        };
                    }

                    // Deduct the balance
                    wallet.Balance -= 50000;
                    await _unitOfWork.WalletRepository.UpdateAsync(wallet);

                    // Map DTO to entity
                    var learningRegis = _mapper.Map<Learning_Registration>(createLearningRegisDTO);
                    learningRegis.Status = LearningRegis.Pending;

                    // Add learning registration
                    await _unitOfWork.LearningRegisRepository.AddAsync(learningRegis);
                    await _unitOfWork.SaveChangeAsync();

                    // Add learning registration days
                    if (createLearningRegisDTO.LearningDays != null && createLearningRegisDTO.LearningDays.Any())
                    {
                        var learningDays = createLearningRegisDTO.LearningDays.Select(day => new LearningRegistrationDay
                        {
                            LearningRegisId = learningRegis.LearningRegisId,
                            DayOfWeek = day
                        }).ToList();

                        await _unitOfWork.LearningRegisDayRepository.AddRangeAsync(learningDays);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    // Create a wallet transaction
                    var walletTransaction = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        WalletId = wallet.WalletId,
                        Amount = 50000,
                        TransactionType = TransactionType.Payment,
                        Status = Model.Enum.TransactionStatus.Complete,
                        TransactionDate = DateTime.UtcNow
                    };

                    await _unitOfWork.WalletTransactionRepository.AddAsync(walletTransaction);
                    await _unitOfWork.SaveChangeAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();

                    _logger.LogInformation("Learning registration added successfully. Wallet balance updated.");

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Learning Registration added successfully. Wallet balance updated. Status set to Pending."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing learning registration.");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
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

        public async Task<ResponseDTO> GetRegistrationsByLearnerIdAsync(int learnerId)
        {
            var registrations = await _learningRegisRepository.GetRegistrationsByLearnerIdAsync(learnerId);
            var registrationDtos = _mapper.Map<IEnumerable<OneOnOneRegisDTO>>(registrations);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = $"All registrations for Learner ID {learnerId} retrieved successfully.",
                Data = registrationDtos
            };
        }

        public async Task<ResponseDTO> UpdateLearningRegisStatusAsync(UpdateLearningRegisDTO updateDTO)
        {

            try
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

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                _mapper.Map(updateDTO, learningRegis);
                learningRegis.Status = LearningRegis.Accepted;

                await _unitOfWork.LearningRegisRepository.UpdateAsync(learningRegis);
                await _unitOfWork.SaveChangeAsync();

                await _unitOfWork.CommitTransactionAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Learning Registration updated successfully."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to update Learning Registration. " + ex.Message
                };
            }
        }

    }
}
