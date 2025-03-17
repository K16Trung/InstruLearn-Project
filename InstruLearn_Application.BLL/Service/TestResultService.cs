using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Models.DTO.Test_Result;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO.Feedback;

namespace InstruLearn_Application.BLL.Service
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepository _testResultRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TestResultService(ITestResultRepository testResultRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _testResultRepository = testResultRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<ResponseDTO>> GetAllTestResultsAsync()
        {
            var testResultList = await _unitOfWork.TestResultRepository.GetAllAsync();
            var testResultDtos = _mapper.Map<IEnumerable<TestResultDTO>>(testResultList);

            var responseList = new List<ResponseDTO>();

            foreach (var testResultDto in testResultDtos)
            {
                responseList.Add(new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Syllabus retrieved successfully.",
                    Data = testResultDto
                });
            }
            return responseList;
        }
        public async Task<ResponseDTO> GetTestResultByIdAsync(int testResultId)
        {
            var testResult = await _unitOfWork.TestResultRepository.GetByIdAsync(testResultId);
            if (testResult == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Test Result not found.",
                };
            }
            var testResultDto = _mapper.Map<TestResultDTO>(testResult);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Test Result retrieved successfully.",
                Data = testResultDto
            };
        }
        public async Task<ResponseDTO> CreateTestResultAsync(CreateTestResultDTO createTestResultDTO)
        {
            var learner = await _unitOfWork.LearnerRepository.GetByIdAsync(createTestResultDTO.LearnerId);
            if (learner == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Learner not found",
                };
            }
            var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(createTestResultDTO.TeacherId);
            if (teacher == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Teacher not found",
                };
            }
            var majorTest = await _unitOfWork.MajorTestRepository.GetByIdAsync(createTestResultDTO.MajorTestId);
            if (majorTest == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Major Test not found",
                };
            }
            var testResult = _mapper.Map<Test_Result>(createTestResultDTO);
            testResult.Learner = learner;
            testResult.Teacher = teacher;
            testResult.MajorTest = majorTest;

            await _unitOfWork.TestResultRepository.AddAsync(testResult);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Test Result created successfully.",
                Data = _mapper.Map<TestResultDTO>(testResult)
            };
        }
        public async Task<ResponseDTO> DeleteTestResultAsync(int testResultId)
        {
            var deleteTestResult = await _unitOfWork.TestResultRepository.GetByIdAsync(testResultId);
            if (deleteTestResult != null)
            {
                await _unitOfWork.TestResultRepository.DeleteAsync(testResultId);
                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Test Result deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Test Result with ID {testResultId} not found"
                };
            }
        }
    }
}
