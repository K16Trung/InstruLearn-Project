using InstruLearn_Application.Model.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IRevenueService
    {
        Task<ResponseDTO> GetTotalRevenueAsync();
        Task<ResponseDTO> GetMonthlyRevenueAsync(int year);
        Task<ResponseDTO> GetWeeklyRevenueAsync(int year, int weekNumber);
        Task<ResponseDTO> GetDailyRevenueAsync(DateTime date);
    }
}