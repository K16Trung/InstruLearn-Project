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
                    Message = $"Đã truy xuất {selfAssessmentDTOs.Count} bài tự đánh giá.",
                    Data = selfAssessmentDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all self assessments");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất các bài tự đánh giá: {ex.Message}"
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
                        Message = $"Không tìm thấy bài tự đánh giá với ID {id}."
                    };
                }

                var selfAssessmentDTO = _mapper.Map<SelfAssessmentDTO>(selfAssessment);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã truy xuất bài tự đánh giá thành công.",
                    Data = selfAssessmentDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving self assessment {SelfAssessmentId}", id);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi truy xuất bài tự đánh giá: {ex.Message}"
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
                    Message = "Bài tự đánh giá đã được tạo thành công.",
                    Data = selfAssessmentDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating self assessment");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tạo bài tự đánh giá: {ex.Message}"
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
                        Message = $"Không tìm thấy bài tự đánh giá với ID {id}."
                    };
                }

                _mapper.Map(updateDTO, existingSelfAssessment);

                await _unitOfWork.SelfAssessmentRepository.UpdateAsync(existingSelfAssessment);
                await _unitOfWork.SaveChangeAsync();

                var selfAssessmentDTO = _mapper.Map<SelfAssessmentDTO>(existingSelfAssessment);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Bài tự đánh giá đã được cập nhật thành công.",
                    Data = selfAssessmentDTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating self assessment {SelfAssessmentId}", id);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi cập nhật bài tự đánh giá: {ex.Message}"
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
                        Message = $"Không tìm thấy bài tự đánh giá với ID {id}."
                    };
                }

                if (selfAssessment.LearningRegistrations != null && selfAssessment.LearningRegistrations.Any())
                {
                    await _unitOfWork.SelfAssessmentRepository.UpdateAsync(selfAssessment);
                    await _unitOfWork.SaveChangeAsync();

                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = $"Bài tự đánh giá với ID {id} đã bị vô hiệu hóa vì nó liên kết với các đăng ký học tập."
                    };
                }

                await _unitOfWork.SelfAssessmentRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Bài tự đánh giá với ID {id} đã được xóa thành công."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting self assessment {SelfAssessmentId}", id);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi xóa bài tự đánh giá: {ex.Message}"
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
                        Message = $"Không tìm thấy bài tự đánh giá với ID {id}."
                    };
                }

                var selfAssessmentDTO = _mapper.Map<SelfAssessmentDTO>(selfAssessment);
                var learningRegisDTOs = _mapper.Map<List<object>>(selfAssessment.LearningRegistrations);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã truy xuất bài tự đánh giá thành công.",
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
                    Message = $"Lỗi khi truy xuất bài tự đánh giá: {ex.Message}"
                };
            }
        }

    }
}
