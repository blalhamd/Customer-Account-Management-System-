using CAMS.API.Extensions;
using CAMS.API.Filters.Authentication;
using CAMS.API.Helpers;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.Transaction;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // POST: api/transactions
        [HttpPost]
        [RequiredPermission(Permissions.Transactions.Create)]
        public async Task<IActionResult> CreateTransactionAsync([FromBody] CreateTransactionDto dto, CancellationToken ct)
        {
            string? userId = Request.GetUserId();

            if (userId is null)
                return Unauthorized($"User ID not found in claims");

            var transaction = await _transactionService.CreateTransactionAsync(userId,dto, ct);

            return Ok(transaction);
        }

        // GET: api/transactions/{transactionId}
        [HttpGet("{transactionId}")]
        [RequiredPermission(Permissions.Transactions.ViewById)]
        public async Task<IActionResult> GetTransactionByIdAsync([FromRoute] string transactionId, CancellationToken ct)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(transactionId, ct);

            return Ok(transaction);
        }

        // GET: api/transactions
        [HttpGet]
        [RequiredPermission(Permissions.Transactions.View)]
        public async Task<IActionResult> GetTransactionsAsync([FromQuery] TransactionQuery query, CancellationToken ct)
        {
            var transactions = await _transactionService.GetTransactionsAsync(query, ct);
            return Ok(transactions);
        }
    }
}
