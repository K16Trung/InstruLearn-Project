using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.ResponseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ResponseTypeService : IResponseTypeService
    {
        private readonly IResponseTypeRepository _responseTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ResponseTypeService(
            IResponseTypeRepository responseTypeRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseTypeRepository = responseTypeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ResponseDTO>> GetAllResponseType()
        {
            var responseTypeList = await _unitOfWork.ResponseTypeRepository.GetAllAsync();
            var responseTypeDtos = _mapper.Map<IEnumerable<ReponseTypeDTO>>(responseTypeList);

            var responseList = new List<ResponseDTO>();
            foreach (var responseTypeDto in responseTypeDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Response type retrieved successfully.",
                    Data = responseTypeDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> GetResponseTypeById(int responseTypeId)
        {
            var responseType = await _unitOfWork.ResponseTypeRepository.GetByIdAsync(responseTypeId);
            if (responseType == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Response type not found.",
                };
            }
            var responseTypeDto = _mapper.Map<ReponseTypeDTO>(responseType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Response type retrieved successfully.",
                Data = responseTypeDto
            };
        }

        public async Task<ResponseDTO> CreateResponseType(CreateResponseTypeDTO createResponseTypeDTO)
        {
            var responseType = _mapper.Map<ResponseType>(createResponseTypeDTO);
            await _unitOfWork.ResponseTypeRepository.AddAsync(responseType);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Response type created successfully.",
            };
        }

        public async Task<ResponseDTO> UpdateResponseType(int responseTypeId, UpdateResponseTypeDTO updateResponseTypeDTO)
        {
            var responseTypeUpdate = await _unitOfWork.ResponseTypeRepository.GetByIdAsync(responseTypeId);
            if (responseTypeUpdate != null)
            {
                responseTypeUpdate = _mapper.Map(updateResponseTypeDTO, responseTypeUpdate);
                await _unitOfWork.ResponseTypeRepository.UpdateAsync(responseTypeUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Response type updated successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Response type update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Response type not found!"
            };
        }

        public async Task<ResponseDTO> DeleteResponseType(int responseTypeId)
        {
            var deleteResponseType = await _unitOfWork.ResponseTypeRepository.GetByIdAsync(responseTypeId);
            if (deleteResponseType != null)
            {
                await _unitOfWork.ResponseTypeRepository.DeleteAsync(responseTypeId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Response type deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Response type with ID {responseTypeId} not found"
                };
            }
        }
    }
}