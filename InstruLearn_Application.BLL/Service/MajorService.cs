using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
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
                Message = "Đã lấy chuyên ngành thành công.",
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
                    Message = "Không tìm thấy chuyên ngành."
                };
            }
            var majorDtos = _mapper.Map<MajorDTO>(major);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã lấy chuyên ngành thành công.",
                Data = majorDtos
            };
        }

        public async Task<ResponseDTO> AddMajorAsync(CreateMajorDTO createDto)
        {
            var major = _mapper.Map<Major>(createDto);
            major.Status = MajorStatus.Available;
            await _unitOfWork.MajorRepository.AddAsync(major);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã thêm chuyên ngành thành công.",
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
                    Message = "Không tìm thấy chuyên ngành."
                };
            }
            _mapper.Map(updateDto, existingMajor);
            await _unitOfWork.MajorRepository.UpdateAsync(existingMajor);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã cập nhật chuyên ngành thành công."
            };
        }
        public async Task<ResponseDTO> UpdateStatusMajorAvailableAsync(int majorId)
        {
            var response = new ResponseDTO();

            var major = await _unitOfWork.MajorRepository.GetByIdAsync(majorId);
            if (major == null)
            {
                response.Message = "Không tìm thấy chuyên ngành.";
                return response;
            }

            major.Status = MajorStatus.Available;

            var updated = await _unitOfWork.MajorRepository.UpdateAsync(major);

            if (!updated)
            {
                response.Message = "Thay đổi trạng thái thất bại.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Thay đổi trạng thái chuyên ngành thành công!";
            return response;
        }

        public async Task<ResponseDTO> UpdateStatusMajorUnavailableAsync(int majorId)
        {
            var response = new ResponseDTO();

            var major = await _unitOfWork.MajorRepository.GetByIdAsync(majorId);
            if (major == null)
            {
                response.Message = "Không tìm thấy chuyên ngành.";
                return response;
            }

            major.Status = MajorStatus.Unavailable;

            var updated = await _unitOfWork.MajorRepository.UpdateAsync(major);

            if (!updated)
            {
                response.Message = "Thay đổi trạng thái thất bại.";
                return response;
            }

            response.IsSucceed = true;
            response.Message = "Thay đổi trạng thái chuyên ngành thành công!";
            return response;
        }

        public async Task<ResponseDTO> DeleteMajorAsync(int majorId)
        {
            var major = await _unitOfWork.MajorRepository.GetByIdAsync(majorId);
            if (major == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy chuyên ngành."
                };
            }
            await _unitOfWork.MajorRepository.DeleteAsync(majorId);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã xóa chuyên ngành thành công."
            };
        }
    }
}
