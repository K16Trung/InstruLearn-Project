using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IRevenueService
    {
        Task<ResponseDTO> GetTotalRevenueAsync();
        Task<ResponseDTO> GetMonthlybyYearRevenueAsync(int year);
        Task<ResponseDTO> GetMonthlyRevenueWithWeeksAsync(int year, int month);
        Task<ResponseDTO> GetDailyRevenueAsync(DateTime date);
    }
}
