using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Test_Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ITestResultService
    {
        Task<List<ResponseDTO>> GetAllTestResultsAsync();
        Task<ResponseDTO> GetTestResultByIdAsync(int testResultId);
        Task<ResponseDTO> CreateTestResultAsync(CreateTestResultDTO createTestResultDTO);
        Task<ResponseDTO> DeleteTestResultAsync(int testResultId);
    }
}
