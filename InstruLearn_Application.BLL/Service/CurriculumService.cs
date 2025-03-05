using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Curriculum;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class CurriculumService : ICurriculumService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CurriculumService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<CurriculumDTO>> GetAllCurriculumAsync()
        {
            var curriculumGetAll = await _unitOfWork.CurriculumRepository.GetAllAsync();
            var curriculumMapper = _mapper.Map<List<CurriculumDTO>>(curriculumGetAll);
            return curriculumMapper;
        }
        public async Task<CurriculumDTO> GetCurriculumByIdAsync(int curriculumId)
        {
            var curriculumGetById = await _unitOfWork.CurriculumRepository.GetByIdAsync(curriculumId);
            var curriculumMapper = _mapper.Map<CurriculumDTO>(curriculumGetById);
            return curriculumMapper;
        }
        public async Task<ResponseDTO> AddCurriculumAsync(CreateCurriculumDTO createCurriculumDTO)
        {
            var centerCourse = await _unitOfWork.CenterCourseRepository.GetByIdAsync(createCurriculumDTO.CenterCourseId);
            if (centerCourse == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "CenterCourse not found",
                };
            }
            var curriculumObj = _mapper.Map<Curriculum>(createCurriculumDTO);
            curriculumObj.CenterCourse = centerCourse;
            await _unitOfWork.CurriculumRepository.AddAsync(curriculumObj);
            await _unitOfWork.SaveChangeAsync();
            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Curriculum added successfully",
            };
            return response;
        }
        public async Task<ResponseDTO> UpdateCurriculumAsync(int curriculumId, UpdateCurriculumDTO updateCurriculumDTO)
        {
            var curriculumUpdate = await _unitOfWork.CurriculumRepository.GetByIdAsync(curriculumId);
            if (curriculumUpdate != null)
            {
                curriculumUpdate = _mapper.Map(updateCurriculumDTO, curriculumUpdate);
                await _unitOfWork.CurriculumRepository.UpdateAsync(curriculumUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Curriculum update successfully! "
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Curriculum update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Curriculum not found!"
            };
        }
        public async Task<ResponseDTO> DeleteCurriculumAsync(int curriculumId)
        {
            var deleteCurriculum = await _unitOfWork.CurriculumRepository.GetByIdAsync(curriculumId);
            if (deleteCurriculum != null)
            {
                await _unitOfWork.CurriculumRepository.DeleteAsync(curriculumId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Curriculum deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Curriculum with ID {curriculumId} not found"
                };
            }
        }
    }
}
