using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService _revenueService;

        public RevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var result = await _revenueService.GetTotalRevenueAsync();
            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("monthly/{year}")]
        public async Task<IActionResult> GetMonthlyRevenue(int year)
        {
            if (year < 2000 || year > 2050)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Năm phải hợp lệ (2000-2050)."
                });
            }

            var result = await _revenueService.GetMonthlyRevenueAsync(year);
            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("weekly/{year}/{weekNumber}")]
        public async Task<IActionResult> GetWeeklyRevenue(int year, int weekNumber)
        {
            if (year < 2000 || year > 2050)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Năm phải hợp lệ (2000-2050)."
                });
            }

            if (weekNumber < 1 || weekNumber > 53)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Số tuần phải hợp lệ (1-53)."
                });
            }

            var result = await _revenueService.GetWeeklyRevenueAsync(year, weekNumber);
            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyRevenue([FromQuery] DateTime date)
        {
            var result = await _revenueService.GetDailyRevenueAsync(date);
            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}