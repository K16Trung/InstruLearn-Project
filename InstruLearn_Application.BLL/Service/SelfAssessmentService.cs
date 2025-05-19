using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.SelfAssessment;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class SelfAssessmentService : ISelfAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SelfAssessmentService> _logger;

        public SelfAssessmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SelfAssessmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDTO> GetAllAsync()
        {
            try
            {
                var selfAssessments = await _unitOfWork.SelfAssessmentRepository.GetAllAsync();
                var selfAssessmentDTOs = _mapper.Map<List<SelfAssessmentDTO>>(selfAssessments);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Retrieved {selfAssessmentDTOs.Count} self assessments.",
                    Data = selfAssessmentDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all self assessments");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving self assessments: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO> GetByIdAsync(int id)
        {
            try
            {
                var selfAssessment = await _unitOfWork.SelfAssessmentRepository.GetByIdAsync(id);

                if (selfAssessment == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Self assessment with ID {id} not found."
                    };
                }

                var selfAssessmentDTO = _mapper.Map<SelfAssessmentDTO>(selfAssessment);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Self assessment retrieved successfully.",
                    Data = selfAssessmentDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving self assessment {SelfAssessmentId}", id);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving self assessment: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateAsync(CreateSelfAssessmentDTO createDTO)
        {
            try
            {
                var selfAssessment = _mapper.Map<SelfAssessment>(createDTO);

                await _unitOfWork.SelfAssessmentRepository.AddAsync(selfAssessment);
                await _unitOfWork.SaveChangeAsync();

                var selfAssessmentDTO = _mapper.Map<SelfAssessmentDTO>(selfAssessment);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Self assessment created successfully.",
                    Data = selfAssessmentDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating self assessment");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error creating self assessment: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateAsync(int id, UpdateSelfAssessmentDTO updateDTO)
        {
            try
            {
                var existingSelfAssessment = await _unitOfWork.SelfAssessmentRepository.GetByIdAsync(id);

                if (existingSelfAssessment == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Self assessment with ID {id} not found."
                    };
                }

                _mapper.Map(updateDTO, existingSelfAssessment);

                await _unitOfWork.SelfAssessmentRepository.UpdateAsync(existingSelfAssessment);
                await _unitOfWork.SaveChangeAsync();

                var selfAssessmentDTO = _mapper.Map<SelfAssessmentDTO>(existingSelfAssessment);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Self assessment updated successfully.",
                    Data = selfAssessmentDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating self assessment {SelfAssessmentId}", id);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error updating self assessment: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> DeleteAsync(int id)
        {
            try
            {
                var selfAssessment = await _unitOfWork.SelfAssessmentRepository.GetByIdWithRegistrationsAsync(id);

                if (selfAssessment == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Self assessment with ID {id} not found."
                    };
                }

                if (selfAssessment.LearningRegistrations != null && selfAssessment.LearningRegistrations.Any())
                {
                    await _unitOfWork.SelfAssessmentRepository.UpdateAsync(selfAssessment);
                    await _unitOfWork.SaveChangeAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Self assessment with ID {id} has been deactivated because it is associated with learning registrations."
                    };
                }

                await _unitOfWork.SelfAssessmentRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Self assessment with ID {id} has been deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting self assessment {SelfAssessmentId}", id);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error deleting self assessment: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetByIdWithRegistrationsAsync(int id)
        {
            try
            {
                var selfAssessment = await _unitOfWork.SelfAssessmentRepository.GetByIdWithRegistrationsAsync(id);

                if (selfAssessment == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Self assessment with ID {id} not found."
                    };
                }

                var selfAssessmentDTO = _mapper.Map<SelfAssessmentDTO>(selfAssessment);
                var learningRegisDTOs = _mapper.Map<List<object>>(selfAssessment.LearningRegistrations);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Self assessment retrieved successfully.",
                    Data = new
                    {
                        SelfAssessment = selfAssessmentDTO,
                        LearningRegistrations = learningRegisDTOs
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving self assessment with registrations {SelfAssessmentId}", id);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Error retrieving self assessment: {ex.Message}"
                };
            }
        }

    }
}
