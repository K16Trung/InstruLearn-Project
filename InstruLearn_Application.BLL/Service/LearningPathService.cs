using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningPathSession;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningPathService : ILearningPathService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LearningPathService> _logger;
        private readonly IMapper _mapper;

        public LearningPathService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LearningPathService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseDTO> GetLearningPathSessionsAsync(int learningRegisId)
        {
            try
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

                var learningPathSessions = await _unitOfWork.LearningPathSessionRepository.GetByLearningRegisIdAsync(learningRegisId);
                var learningPathSessionDTOs = _mapper.Map<List<LearningPathSessionDTO>>(learningPathSessions);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Learning path sessions retrieved successfully.",
                    Data = learningPathSessionDTOs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learning path sessions for registration {LearningRegisId}", learningRegisId);
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to retrieve learning path sessions: " + ex.Message
                };
            }
        }

        public async Task<ResponseDTO> UpdateSessionCompletionStatusAsync(int learningPathSessionId, bool isCompleted)
        {
            try
            {
                var session = await _unitOfWork.LearningPathSessionRepository.GetByIdAsync(learningPathSessionId);

                if (session == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Learning path session not found."
                    };
                }

                // Update completion status
                session.IsCompleted = isCompleted;
                await _unitOfWork.LearningPathSessionRepository.UpdateAsync(session);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Session completion status {(isCompleted ? "marked as completed" : "marked as incomplete")} successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning path session completion status");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Failed to update session completion status: " + ex.Message
                };
            }
        }

    }
}
