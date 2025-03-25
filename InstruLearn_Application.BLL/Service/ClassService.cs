using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Class;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ClassDTO>> GetAllClassAsync()
        {
            var ClassGetAll = await _unitOfWork.ClassRepository.GetAllAsync();
            var ClassMapper = _mapper.Map<List<ClassDTO>>(ClassGetAll);
            return ClassMapper;
        }

        public async Task<ClassDTO> GetClassByIdAsync(int classId)
        {
            var ClassGetById = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            var ClassMapper = _mapper.Map<ClassDTO>(ClassGetById);
            return ClassMapper;
        }

        public async Task<ResponseDTO> AddClassAsync(CreateClassDTO createClassDTO)
        {
            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(createClassDTO.TeacherId);
            if (teacher == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy giáo viên",
                };
            }
            var coursePackage = await _unitOfWork.CourseRepository.GetByIdAsync(createClassDTO.CoursePackageId);
            if (teacher == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Không tìm thấy gói học",
                };
            }

            var classObj = _mapper.Map<Class>(createClassDTO);
            classObj.Teacher = teacher;
            classObj.CoursePackage = coursePackage;

            await _unitOfWork.ClassRepository.AddAsync(classObj);

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Đã thêm lớp thành công",
            };

            return response;
        }

        public async Task<ResponseDTO> UpdateClassAsync(int classId, UpdateClassDTO updateClassDTO)
        {
            var classUpdate = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (classUpdate != null)
            {
                classUpdate = _mapper.Map(updateClassDTO, classUpdate);
                await _unitOfWork.ClassRepository.UpdateAsync(classUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Đã cập nhật lớp thành công!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Cập nhật lớp thất bại!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Không tìm thấy lớp!"
            };
        }

        public async Task<ResponseDTO> DeleteClassAsync(int classId)
        {
            var deleteFeedback = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (deleteFeedback != null)
            {
                await _unitOfWork.ClassRepository.DeleteAsync(classId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Đã xóa lớp thành công"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Không tìm thấy lớp có ID {classId}"
                };
            }
        }
    }
}
