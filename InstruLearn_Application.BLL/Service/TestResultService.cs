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

        // getall
        public async Task<List<ResponseDTO>> GetAllTestResultsAsync()
        {
            var testResultList = await _unitOfWork.TestResultRepository.GetAllWithDetailsAsync();
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

        // get by id
        public async Task<ResponseDTO> GetTestResultByIdAsync(int testResultId)
        {
            var testResult = await _unitOfWork.TestResultRepository.GetByIdWithDetailsAsync(testResultId);
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

        // create
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
            var major = await _unitOfWork.MajorRepository.GetByIdAsync(createTestResultDTO.MajorId ?? 0);
            if (major == null)
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
            testResult.Major = major;

            await _unitOfWork.TestResultRepository.AddAsync(testResult);
            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Test Result created successfully.",
                Data = _mapper.Map<TestResultDTO>(testResult)
            };
        }

        // delete
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

        // get by learning regis id

        public async Task<ResponseDTO> GetTestResultsByLearningRegisIdAsync(int learningRegisId)
        {
            var testResults = await _unitOfWork.TestResultRepository.GetTestResultsByLearningRegisIdAsync(learningRegisId);

            if (testResults == null || !testResults.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "No test results found for the given Learning Registration ID."
                };
            }

            // Optionally map to DTO (if avoiding returning full EF entities)
            var testResultsDTO = _mapper.Map<List<TestResultDTO>>(testResults);

            return new ResponseDTO
            {
                IsSucceed = true,
                Data = testResultsDTO
            };
        }

        // get by learner id

        public async Task<ResponseDTO> GetTestResultsByLearnerIdAsync(int learnerId)
        {
            var testResults = await _unitOfWork.TestResultRepository.GetTestResultsByLearnerIdAsync(learnerId);

            if (testResults == null || !testResults.Any())
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "No test results found for the given Learner ID."
                };
            }

            // Optionally map to DTOs
            var testResultsDTO = _mapper.Map<List<TestResultDTO>>(testResults);

            return new ResponseDTO
            {
                IsSucceed = true,
                Data = testResultsDTO
            };
        }

    }
}
