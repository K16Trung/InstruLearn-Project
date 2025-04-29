using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.WalletTransaction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletTransactionsController : ControllerBase
    {
        private readonly IWalletTransactionService _walletTransactionService;

        public WalletTransactionsController(IWalletTransactionService walletTransactionService)
        {
            _walletTransactionService = walletTransactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {
            var result = await _walletTransactionService.GetAllTransactionsAsync();
            return Ok(result);
        }

        [HttpGet("wallet/{walletId}")]
        public async Task<ActionResult<IEnumerable<WalletTransactionDTO>>> GetTransactionsByWalletId(int walletId)
        {
            var transactions = await _walletTransactionService.GetTransactionsByWalletIdAsync(walletId);
            return Ok(transactions);
        }
    }
}
