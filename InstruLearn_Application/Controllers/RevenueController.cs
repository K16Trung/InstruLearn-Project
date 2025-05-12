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

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearlyRevenueAsync(int year)
        {
            if (year < 2000 || year > 2050)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Năm phải hợp lệ (2000-2050)."
                });
            }

            var result = await _revenueService.GetMonthlybyYearRevenueAsync(year);
            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("monthly/{year}/{month}")]
        public async Task<IActionResult> GetMonthlyRevenueWithWeeks(int year, int month)
        {
            if (year < 2000 || year > 2050)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Năm phải hợp lệ (2000-2050)."
                });
            }

            if (month < 1 || month > 12)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Tháng phải hợp lệ (1-12)."
                });
            }

            var result = await _revenueService.GetMonthlyRevenueWithWeeksAsync(year, month);
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