using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class SyllabusService : ISyllabusService
    {
        private readonly ISyllabusRepository _syllabusRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public SyllabusService(ISyllabusRepository syllabusRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _syllabusRepository = syllabusRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<ResponseDTO>> GetAllSyllabusAsync()
        {
            var syllabusList = await _unitOfWork.SyllabusRepository.GetAllAsync();
            var syllabusDtos = _mapper.Map<IEnumerable<SyllabusDTO>>(syllabusList);

            var responseList = new List<ResponseDTO>();

            foreach (var syllabusDto in syllabusDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Syllabus retrieved successfully.",
                    Data = syllabusDto
                });
            }
            return responseList;
        }
        public async Task<ResponseDTO> GetSyllabusByIdAsync(int syllabusId)
        {
            var syllabus = await _unitOfWork.SyllabusRepository.GetByIdAsync(syllabusId);
            if (syllabus == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Syllabus not found.",
                };
            }
            var syllabusDto = _mapper.Map<SyllabusDTO>(syllabus);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Syllabus retrieved successfully.",
                Data = syllabusDto
            };
        }

        public async Task<ResponseDTO> GetSyllabusByClassIdAsync(int classId)
        {
            var syllabus = await _unitOfWork.SyllabusRepository.GetSyllabusByClassIdAsync(classId);

            if (syllabus == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Syllabus not found for the given class.",
                };
            }

            var syllabusDto = _mapper.Map<SyllabusDTO>(syllabus);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Syllabus retrieved successfully.",
                Data = syllabusDto
            };
        }

        public async Task<ResponseDTO> CreateSyllabusAsync(CreateSyllabusDTO createSyllabusDTO)
        {
            var syllabus = _mapper.Map<Syllabus>(createSyllabusDTO);
            await _unitOfWork.SyllabusRepository.AddAsync(syllabus);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Syllabus created successfully.",
            };
        }
        public async Task<ResponseDTO> UpdateSyllabusAsync(int syllabusId, UpdateSyllabusDTO updateSyllabusDTO)
        {
            var syllabusUpdate = await _unitOfWork.SyllabusRepository.GetByIdAsync(syllabusId);
            if (syllabusUpdate != null)
            {
                syllabusUpdate = _mapper.Map(updateSyllabusDTO, syllabusUpdate);
                await _unitOfWork.SyllabusRepository.UpdateAsync(syllabusUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Syllabus update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Syllabus update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Syllabus not found!"
            };
        }
        public async Task<ResponseDTO> DeleteSyllabusAsync(int syllabusId)
        {
            var deleteSyllabus = await _unitOfWork.SyllabusRepository.GetByIdAsync(syllabusId);
            if (deleteSyllabus != null)
            {
                await _unitOfWork.SyllabusRepository.DeleteAsync(syllabusId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Syllabus deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Syllabus with ID {syllabusId} not found"
                };
            }
        }
    }
}
