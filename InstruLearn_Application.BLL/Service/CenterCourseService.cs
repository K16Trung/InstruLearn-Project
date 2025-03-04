using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.CenterCourse;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CenterCourseService : ICenterCourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CenterCourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<CenterCourseDTO>> GetAllCenterCourseAsync()
        {
            var centerCourseGetAll = await _unitOfWork.CenterCourseRepository.GetAllAsync();
            var centerCourseMapper = _mapper.Map<List<CenterCourseDTO>>(centerCourseGetAll);
            return centerCourseMapper;
        }
        public async Task<CenterCourseDTO> GetCenterCourseByIdAsync(int centerCourseId)
        {
            var centerCourseGetById = await _unitOfWork.CenterCourseRepository.GetByIdAsync(centerCourseId);
            var centerCourseMapper = _mapper.Map<CenterCourseDTO>(centerCourseGetById);
            return centerCourseMapper;
        }
        public async Task<ResponseDTO> AddCenterCourseAsync(CreateCenterCourseDTO createCenterCourseDTO)
        {

            var centerCourseObj = _mapper.Map<Center_Course>(createCenterCourseDTO);

            await _unitOfWork.CenterCourseRepository.AddAsync(centerCourseObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "CenterCourse added successfully",
            };

            return response;
        }
        public async Task<ResponseDTO> UpdateCenterCourseAsync(int centerCourseId, UpdateCenterCourseDTO updateCenterCourseDTO)
        {
            var centerCourseUpdate = await _unitOfWork.CenterCourseRepository.GetByIdAsync(centerCourseId);
            if (centerCourseUpdate != null)
            {
                centerCourseUpdate = _mapper.Map(updateCenterCourseDTO, centerCourseUpdate);
                await _unitOfWork.CenterCourseRepository.UpdateAsync(centerCourseUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "CenterCourse update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "CenterCourse update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "CenterCourse not found!"
            };
        }
        public async Task<ResponseDTO> DeleteCenterCourseAsync(int centerCourseId)
        {
            var deleteCenterCourse = await _unitOfWork.CenterCourseRepository.GetByIdAsync(centerCourseId);
            if (deleteCenterCourse != null)
            {
                await _unitOfWork.FeedbackRepository.DeleteAsync(centerCourseId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "CenterCourse deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"CenterCourse with ID {centerCourseId} not found"
                };
            }
        }
    }
}
