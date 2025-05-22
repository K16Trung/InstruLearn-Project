using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.LevelAssigned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class LevelAssignedService : ILevelAssignedService
    {
        private readonly ILevelAssignedRepository _levelAssignedRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LevelAssignedService(ILevelAssignedRepository levelAssignedRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _levelAssignedRepository = levelAssignedRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ResponseDTO>> GetAllLevelAssigned()
        {
            var levelAssignedList = await _unitOfWork.LevelAssignedRepository.GetAllAsync();
            var levelAssignedDtos = _mapper.Map<IEnumerable<LevelAssignedDTO>>(levelAssignedList);

            var responseList = new List<ResponseDTO>();

            foreach (var levelAssignedDto in levelAssignedDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Level assigned retrieved successfully.",
                    Data = levelAssignedDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> GetLevelAssignedById(int levelAssignedId)
        {
            var levelAssigned = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(levelAssignedId);
            if (levelAssigned == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Level assigned not found.",
                };
            }
            var levelAssignedDto = _mapper.Map<LevelAssignedDTO>(levelAssigned);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Level assigned retrieved successfully.",
                Data = levelAssignedDto
            };
        }

        public async Task<ResponseDTO> CreateLevelAssigned(CreateLevelAssignedDTO createLevelAssignedDTO)
        {
            var levelAssigned = _mapper.Map<LevelAssigned>(createLevelAssignedDTO);
            levelAssigned.SyllabusLink = null;
            await _unitOfWork.LevelAssignedRepository.AddAsync(levelAssigned);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Level assigned created successfully.",
            };
        }

        public async Task<ResponseDTO> UpdateLevelAssigned(int levelAssignedId, UpdateLevelAssignedDTO updateLevelAssignedDTO)
        {
            var levelAssignedUpdate = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(levelAssignedId);
            if (levelAssignedUpdate != null)
            {
                bool hasChanges = levelAssignedUpdate.LevelPrice != updateLevelAssignedDTO.LevelPrice ||
                                  levelAssignedUpdate.SyllabusLink != updateLevelAssignedDTO.SyllabusLink;

                levelAssignedUpdate.LevelPrice = updateLevelAssignedDTO.LevelPrice;
                levelAssignedUpdate.SyllabusLink = updateLevelAssignedDTO.SyllabusLink;

                var result = await _unitOfWork.SaveChangeAsync();

                if (result > 0 || !hasChanges)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Level assigned updated successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Level assigned update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Level assigned not found!"
            };
        }

        public async Task<ResponseDTO> UpdateSyllabusLink(int levelAssignedId, UpdateSyllabusLinkDTO updateSyllabusLinkDTO)
        {
            var levelAssigned = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(levelAssignedId);
            if (levelAssigned != null)
            {
                levelAssigned.SyllabusLink = updateSyllabusLinkDTO.SyllabusLink;
                var updateResult = await _unitOfWork.LevelAssignedRepository.UpdateAsync(levelAssigned);
                if (updateResult)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Syllabus link updated successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Syllabus link update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Level assigned not found!"
            };
        }

        public async Task<ResponseDTO> DeleteLevelAssigned(int levelAssignedId)
        {
            var deleteLevelAssigned = await _unitOfWork.LevelAssignedRepository.GetByIdAsync(levelAssignedId);
            if (deleteLevelAssigned != null)
            {
                await _unitOfWork.LevelAssignedRepository.DeleteAsync(levelAssignedId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Level assigned deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Level assigned with ID {levelAssignedId} not found"
                };
            }
        }
    }
}