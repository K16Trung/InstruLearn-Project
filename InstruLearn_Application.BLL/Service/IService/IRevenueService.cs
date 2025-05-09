using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IRevenueService
    {
        Task<ResponseDTO> GetTotalRevenueAsync();
        Task<ResponseDTO> GetRevenueByTimeRangeAsync(DateTime startDate, DateTime endDate);
        Task<ResponseDTO> GetRevenueByTypeAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<ResponseDTO> GetMonthlyRevenueAsync(int year);
    }
}