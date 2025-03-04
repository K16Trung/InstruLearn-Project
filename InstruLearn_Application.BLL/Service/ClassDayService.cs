using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.ClassDay;

namespace InstruLearn_Application.BLL.Service
{
    public class ClassDayService : IClassDayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassDayService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<ClassDayDTO>> GetAllClassDayAsync()
        {
            var classDayGetAll = await _unitOfWork.ClassDayRepository.GetAllAsync();
            var classDayMapper = _mapper.Map<List<ClassDayDTO>>(classDayGetAll);
            return classDayMapper;
        }
        public async Task<ClassDayDTO> GetClassDayByIdAsync(int classDayId)
        {
            var classDayGetById = await _unitOfWork.ClassDayRepository.GetByIdAsync(classDayId);
            var classDayMapper = _mapper.Map<ClassDayDTO>(classDayGetById);
            return classDayMapper;
        }
        public async Task<ResponseDTO> AddClassDayAsync(CreateClassDayDTO createClassDayDTO)
        {
            var classfind = await _unitOfWork.ClassRepository.GetByIdAsync(createClassDayDTO.ClassId);
            if (classfind == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Class not found",
                };
            }

            var classObj = _mapper.Map<ClassDay>(createClassDayDTO);
            classObj.Class = classfind;

            await _unitOfWork.ClassDayRepository.AddAsync(classObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "ClassDay added successfully",
            };

            return response;
        }
        public async Task<ResponseDTO> UpdateClassDayAsync(int classDayId, UpdateClassDayDTO updateClassDayDTO)
        {
            var classDayUpdate = await _unitOfWork.ClassDayRepository.GetByIdAsync(classDayId);
            if (classDayUpdate != null)
            {
                classDayUpdate = _mapper.Map(updateClassDayDTO, classDayUpdate);
                await _unitOfWork.ClassDayRepository.UpdateAsync(classDayUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Class update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Class update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Class not found!"
            };
        }
        public async Task<ResponseDTO> DeleteClassDayAsync(int classDayId)
        {
            var deleteClassDay = await _unitOfWork.ClassDayRepository.GetByIdAsync(classDayId);
            if (deleteClassDay != null)
            {
                await _unitOfWork.ClassDayRepository.DeleteAsync(classDayId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "ClassDay deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"ClassDay with ID {classDayId} not found"
                };
            }
        }
    }
}
