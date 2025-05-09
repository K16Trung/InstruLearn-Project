using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.MajorTest;
using InstruLearn_Application.Model.Models.DTO.Test_Result;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Feedback;

namespace InstruLearn_Application.BLL.Service
{
    public class MajorTestService : IMajorTestService
    {
        private readonly IMajorTestRepository _majorTestRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MajorTestService(IMajorTestRepository majorTestRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _majorTestRepository = majorTestRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ResponseDTO>> GetAllMajorTestsAsync()
        {
            var majorTestList = await _unitOfWork.MajorTestRepository.GetAllAsync();
            var majorTestDtos = _mapper.Map<IEnumerable<MajorTestDTO>>(majorTestList);

            var responseList = new List<ResponseDTO>();

            foreach (var majorTestDto in majorTestDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Major test retrieved successfully.",
                    Data = majorTestDto
                });
            }
            return responseList;
        }

        public async Task<ResponseDTO> GetMajorTestByIdAsync(int majorTestId)
        {
            var majorTest = await _unitOfWork.MajorTestRepository.GetByIdAsync(majorTestId);
            if (majorTest == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Major Test not found.",
                };
            }
            var majorTestDto = _mapper.Map<MajorTestDTO>(majorTest);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Major Test retrieved successfully.",
                Data = majorTestDto
            };
        }

        public async Task<ResponseDTO> GetMajorTestsByMajorIdAsync(int majorId)
        {
            var majorTests = await _unitOfWork.MajorTestRepository.GetMajorTestsByMajorIdAsync(majorId);

            if (majorTests == null || !majorTests.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "No Major Tests found for this MajorId."
                };
            }

            var majorTestDtos = _mapper.Map<List<MajorTestDTO>>(majorTests);

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Major Tests retrieved successfully.",
                Data = majorTestDtos
            };
        }

        public async Task<ResponseDTO> CreateMajorTestAsync(CreateMajorTestDTO createMajorTestDTO)
        {
            var majorObj = await _unitOfWork.MajorRepository.GetByIdAsync(createMajorTestDTO.MajorId);
            if (majorObj == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Major not found",
                };
            }
            var majorTestDto = _mapper.Map<MajorTest>(createMajorTestDTO);
            majorTestDto.Major = majorObj;

            await _unitOfWork.MajorTestRepository.AddAsync(majorTestDto);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Test Result created successfully.",
                Data = _mapper.Map<MajorTestDTO>(majorTestDto)
            };
        }

        public async Task<ResponseDTO> UpdateMajorTestAsync(int majorTestId, UpdateMajorTestDTO updateMajorTestDTO)
        {
            var majorTestUpdate = await _unitOfWork.MajorTestRepository.GetByIdAsync(majorTestId);
            if (majorTestUpdate != null)
            {
                majorTestUpdate = _mapper.Map(updateMajorTestDTO, majorTestUpdate);
                await _unitOfWork.MajorTestRepository.UpdateAsync(majorTestUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Major Test update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Major Test update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Major Test not found!"
            };
        }

        public async Task<ResponseDTO> DeleteMajorTestAsync(int majorTestId)
        {
            var deleteMajorTest = await _unitOfWork.MajorTestRepository.GetByIdAsync(majorTestId);
            if (deleteMajorTest != null)
            {
                await _unitOfWork.MajorTestRepository.DeleteAsync(majorTestId);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Major Test deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Major Test with ID {majorTestId} not found"
                };
            }

        }
    }
}
