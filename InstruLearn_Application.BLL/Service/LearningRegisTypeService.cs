using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LearningRegistration;
using InstruLearn_Application.Model.Models.DTO.LearningRegistrationType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class LearningRegisTypeService : ILearningRegisTypeService
    {
        private readonly ILearningRegisTypeRepository _learningRegisTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LearningRegisTypeService(ILearningRegisTypeRepository learningRegisTypeRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _learningRegisTypeRepository = learningRegisTypeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> GetAllLearningRegisTypeAsync()
        {
            var type = await _unitOfWork.LearningRegisTypeRepository.GetAllAsync();
            var typeDtos = _mapper.Map<IEnumerable<TypeDTO>>(type);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning Registration Type retrieved successfully.",
                Data = typeDtos
            };
        }
        public async Task<ResponseDTO> GetLearningRegisTypeByIdAsync(int learningRegisTypeId)
        {
            var type = await _unitOfWork.LearningRegisTypeRepository.GetByIdAsync(learningRegisTypeId);
            if (type == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning type Type not found."
                };
            }
            var typeDtos = _mapper.Map<TypeDTO>(type);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning type Type retrieved successfully.",
                Data = typeDtos
            };
        }
        public async Task<ResponseDTO> CreateLearningRegisTypeAsync(CreateTypeDTO createTypeDTO)
        {
            var type = _mapper.Map<Learning_Registration_Type>(createTypeDTO);
            await _unitOfWork.LearningRegisTypeRepository.AddAsync(type);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning type added successfully.",
            };
        }
        public async Task<ResponseDTO> DeleteLearningRegisTypeAsync(int learningRegisTypeId)
        {
            var type = await _unitOfWork.LearningRegisTypeRepository.GetByIdAsync(learningRegisTypeId);
            if (type == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learning type Type not found."
                };
            }
            await _unitOfWork.LearningRegisTypeRepository.DeleteAsync(learningRegisTypeId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Learning type deleted successfully.",
            };
        }
    }
}
