using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using InstruLearn_Application.Model.Models.DTO.Purchase;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllPurchases()
        {
            var result = await _purchaseService.GetAllPurchaseAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchaseById(int id)
        {
            var result = await _purchaseService.GetPurchaseByIdAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseDTO createDto)
        {
            var response = await _purchaseService.CreatePurchaseAsync(createDto);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var result = await _purchaseService.DeletePurchaseAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}