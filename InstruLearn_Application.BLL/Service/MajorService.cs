using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO.Major;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class MajorService : IMajorService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MajorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseDTO> GetAllMajorAsync()
        {
            var major = await _unitOfWork.MajorRepository.GetAllAsync();
            var majorDtos = _mapper.Map<IEnumerable<MajorDTO>>(major);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Major retrieved successfully.",
                Data = majorDtos
            };
        }

        public async Task<ResponseDTO> GetMajorByIdAsync(int majorId)
        {
            var major = await _unitOfWork.MajorRepository.GetByIdAsync(majorId);
            if (major == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Major not found."
                };
            }
            var majorDtos = _mapper.Map<MajorDTO>(major);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Major retrieved successfully.",
                Data = majorDtos
            };
        }

        public async Task<ResponseDTO> AddMajorAsync(CreateMajorDTO createDto)
        {
            var major = _mapper.Map<Major>(createDto);
            await _unitOfWork.MajorRepository.AddAsync(major);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Major added successfully.",
            };
        }

        public async Task<ResponseDTO> UpdateMajorAsync(int majorId, UpdateMajorDTO updateDto)
        {
            var existingMajor = await _unitOfWork.MajorRepository.GetByIdAsync(majorId);
            if (existingMajor == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Major not found."
                };
            }
            _mapper.Map(updateDto, existingMajor);
            await _unitOfWork.MajorRepository.UpdateAsync(existingMajor);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Major updated successfully."
            };
        }

        public async Task<ResponseDTO> DeleteMajorAsync(int majorId)
        {
            var major = await _unitOfWork.MajorRepository.GetByIdAsync(majorId);
            if (major == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Major not found."
                };
            }
            await _unitOfWork.MajorRepository.DeleteAsync(majorId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Major deleted successfully."
            };
        }
    }
}
