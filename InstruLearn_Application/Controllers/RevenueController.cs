using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO;
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

        [HttpGet("range")]
        public async Task<IActionResult> GetRevenueByTimeRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Ngày bắt đầu phải trước ngày kết thúc."
                });
            }

            var result = await _revenueService.GetRevenueByTimeRangeAsync(startDate, endDate);
            if (result.IsSucceed)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("by-type")]
        public async Task<IActionResult> GetRevenueByType([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Ngày bắt đầu phải trước ngày kết thúc."
                });
            }

            var result = await _revenueService.GetRevenueByTypeAsync(startDate, endDate);
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
    }
}