using InstruLearn_Application.BLL.Service;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Purchase;
using InstruLearn_Application.Model.Models.DTO.PurchaseItem;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseItemController : ControllerBase
    {
        private readonly IPurchaseItemService _purchaseItemService;

        public PurchaseItemController(IPurchaseItemService purchaseItemService)
        {
            _purchaseItemService = purchaseItemService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllPurchaseItems()
        {
            var result = await _purchaseItemService.GetAllPurchaseItemAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchaseItemById(int id)
        {
            var result = await _purchaseItemService.GetPurchaseItemByIdAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("by-learner/{id}")]
        public async Task<IActionResult> GetPurchaseItemsByLearnerId(int id)
        {
            var result = await _purchaseItemService.GetPurchaseItemByLearnerIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePurchaseItem([FromBody] CreatePurchaseItemDTO createDto)
        {
            var response = await _purchaseItemService.CreatePurchaseItemAsync(createDto);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePurchaseItem(int id)
        {
            var result = await _purchaseItemService.DeletePurchaseItemAsync(id);
            if (!result.IsSucceed)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}