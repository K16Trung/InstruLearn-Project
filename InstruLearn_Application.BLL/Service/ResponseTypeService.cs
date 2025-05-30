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
                    Message = "Đã lấy loại phản hồi thành công.",
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
                    Message = "Không tìm thấy loại phản hồi.",
                };
            }
            var responseTypeDto = _mapper.Map<ReponseTypeDTO>(responseType);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã lấy loại phản hồi thành công.",
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
                Message = "Đã tạo loại phản hồi thành công.",
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
                        Message = "Đã cập nhật loại phản hồi thành công!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Cập nhật loại phản hồi không thành công!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Không tìm thấy loại phản hồi!"
            };
        }

        public async Task<ResponseDTO> DeleteResponseType(int responseTypeId)
        {
            var deleteResponseType = await _unitOfWork.ResponseTypeRepository.GetByIdAsync(responseTypeId);
            if (deleteResponseType == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy loại phản hồi có ID {responseTypeId}"
                };
            }

            var relatedResponses = await _unitOfWork.ResponseRepository.GetWithIncludesAsync(r => r.ResponseTypeId == responseTypeId);
            if (relatedResponses.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không thể xóa loại phản hồi. Đang có phản hồi vẫn đang sử dụng loại này."
                };
            }

            await _unitOfWork.ResponseTypeRepository.DeleteAsync(responseTypeId);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã xóa loại phản hồi thành công"
            };
        }
    }
}