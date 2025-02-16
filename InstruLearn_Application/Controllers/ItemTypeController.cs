using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.CourseType;
using InstruLearn_Application.Model.Models.DTO.ItemTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemTypeController : ControllerBase
    {
        private readonly IItemTypeService _itemTypeService;

        public ItemTypeController(IItemTypeService itemTypeService)
        {
            _itemTypeService = itemTypeService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllItemType()
        {
            var response = await _itemTypeService.GetAllItemTypeAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemTypeById(int id)
        {
            var response = await _itemTypeService.GetItemTypeByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddItemType([FromBody] CreateItemTypeDTO createDto)
        {
            var response = await _itemTypeService.AddItemTypeAsync(createDto);
            return Ok(response);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateItemType(int id, [FromBody] UpdateItemTypeDTO updateDto)
        {
            var response = await _itemTypeService.UpdateItemTypeAsync(id, updateDto);
            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteItemType(int id)
        {
            var response = await _itemTypeService.DeleteItemTypeAsync(id);
            return Ok(response);
        }
    }
}
