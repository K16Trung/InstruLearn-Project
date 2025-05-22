using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Response;
using InstruLearn_Application.Model.Models.DTO.ResponseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ResponseService : IResponseService
    {
        private readonly IResponseRepository _responseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ResponseService(
            IResponseRepository responseRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseRepository = responseRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ResponseDTO>> GetAllResponseAsync()
        {
            var responses = await _responseRepository.GetAllAsync();
            var responseDtos = responses.Select(r => new ResponseForLearningRegisDTO
            {
                ResponseId = r.ResponseId,
                ResponseDescription = r.ResponseName, // Changed from ResponseName
                ResponseTypes = new List<ReponseTypeDTO>
            {
                _mapper.Map<ReponseTypeDTO>(r.ResponseType)
            }
            });

            var responseList = new List<ResponseDTO>();
            foreach (var responseDto in responseDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Response retrieved successfully.",
                    Data = responseDto
                });
            }

            return responseList;
        }

        public async Task<ResponseDTO> GetResponseByIdAsync(int responseId)
        {
            var response = await _responseRepository.GetByIdAsync(responseId);
            if (response == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Response not found.",
                };
            }

            var responseDto = new ResponseForLearningRegisDTO
            {
                ResponseId = response.ResponseId,
                ResponseDescription = response.ResponseName, // Changed from ResponseName
                ResponseTypes = new List<ReponseTypeDTO>
            {
                _mapper.Map<ReponseTypeDTO>(response.ResponseType)
            }
            };

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Response retrieved successfully.",
                Data = responseDto
            };
        }

        public async Task<ResponseDTO> CreateResponseAsync(CreateResponseDTO createResponseDTO)
        {
            try
            {
                var responseType = await _unitOfWork.ResponseTypeRepository.GetByIdAsync(createResponseDTO.ResponseTypeId);
                if (responseType == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Response Type with ID {createResponseDTO.ResponseTypeId} not found."
                    };
                }

                var response = new Response
                {
                    ResponseTypeId = createResponseDTO.ResponseTypeId,
                    ResponseName = createResponseDTO.ResponseDescription // Changed from ResponseName
                };

                await _unitOfWork.ResponseRepository.AddAsync(response);
                await _unitOfWork.SaveChangeAsync();

                var createdResponse = await _unitOfWork.ResponseRepository.GetByIdAsync(response.ResponseId);

                var responseDto = new ResponseForLearningRegisDTO
                {
                    ResponseId = createdResponse.ResponseId,
                    ResponseDescription = createdResponse.ResponseName, // Changed from ResponseName
                    ResponseTypes = new List<ReponseTypeDTO>
                {
                    _mapper.Map<ReponseTypeDTO>(createdResponse.ResponseType)
                }
                };

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Response created successfully.",
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to create response: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateResponseAsync(int responseId, UpdateResponseDTO updateResponseDTO)
        {
            try
            {
                var response = await _unitOfWork.ResponseRepository.GetByIdAsync(responseId);
                if (response == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Response not found."
                    };
                }

                // Update the ResponseName property using the ResponseDescription from DTO
                response.ResponseName = updateResponseDTO.ResponseDescription; // Changed from ResponseName

                await _unitOfWork.ResponseRepository.UpdateAsync(response);
                await _unitOfWork.SaveChangeAsync();

                var updatedResponse = await _unitOfWork.ResponseRepository.GetByIdAsync(responseId);

                var responseDto = new ResponseForLearningRegisDTO
                {
                    ResponseId = updatedResponse.ResponseId,
                    ResponseDescription = updatedResponse.ResponseName, // Changed from ResponseName
                    ResponseTypes = new List<ReponseTypeDTO>
                {
                    _mapper.Map<ReponseTypeDTO>(updatedResponse.ResponseType)
                }
                };

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Response updated successfully.",
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Failed to update response: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> DeleteResponseAsync(int responseId)
        {
            try
            {
                var response = await _unitOfWork.ResponseRepository.GetByIdAsync(responseId);
                if (response == null)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = "Không tìm thấy phản hồi."
                    };
                }

                var relatedRegistrations = await _unitOfWork.LearningRegisRepository.GetWithIncludesAsync(
                    lr => lr.ResponseId == responseId);

                if (relatedRegistrations.Any())
                {
                    return new ResponseDTO
                    {
                        IsSucceed = false,
                        Message = $"Không thể xóa phản hồi. Phản hồi đang được sử dụng bởi đăng ký học theo yêu cầu."
                    };
                }

                await _unitOfWork.ResponseRepository.DeleteAsync(responseId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Phản hồi đã được xóa thành công."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không xóa được phản hồi: {ex.Message}"
                };
            }
        }
    }
}