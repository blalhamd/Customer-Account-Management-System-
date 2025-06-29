using CAMS.API.Extensions;
using CAMS.API.Filters.Authentication;
using CAMS.API.Helpers;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        // POST: api/loans/apply
        // Apply for loan, clientId from JWT token
        [HttpPost("apply")]
        [RequiredPermission(Permissions.Loans.Apply)]
        public async Task<IActionResult> ApplyForLoanAsync(
            [FromBody] CreateLoanDto dto,
            CancellationToken ct)
        {
            string? clientId = Request.GetUserId();

            if (string.IsNullOrEmpty(clientId))
                return Unauthorized("Client ID not found in token.");

            var success = await _loanService.ApplyForLoanAsync(clientId, dto, ct);

            if (!success)
                return BadRequest("Loan application failed.");

            return Accepted();
        }

        // POST: api/loans/{loanAccountId}/approve
        // Approve loan (admin only, likely)
        [HttpPost("{loanAccountId}/approve")]
        [RequiredPermission(Permissions.Loans.Approve)]
        public async Task<IActionResult> ApproveLoanAsync([FromRoute] string loanAccountId, CancellationToken ct)
        {
            await _loanService.ApproveLoanAsync(loanAccountId, ct);
            return NoContent();
        }

        // POST: api/loans/{loanId}/installment
        // Make an installment payment on a loan
        [HttpPost("{loanId}/installment")]
        [RequiredPermission(Permissions.Loans.Installment)]
        public async Task<IActionResult> MakeInstallmentPaymentAsync(
            [FromRoute] string loanId,
            [FromQuery] decimal amount,
            [FromQuery] string sourceAccountId,
            CancellationToken ct)
        {
            var success = await _loanService.MakeInstallmentPaymentAsync(loanId, amount, sourceAccountId, ct);

            if (!success)
                return BadRequest("Installment payment failed.");

            return NoContent();
        }

        // GET: api/loans
        [HttpGet]
        [RequiredPermission(Permissions.Loans.View)]
        public async Task<IActionResult> GetLoansAsync([FromQuery] LoanQuery query, CancellationToken ct)
        {
            var loans = await _loanService.GetLoansAsync(query, ct);
            return Ok(loans);
        }

        // GET: api/loans/{loanAccountId}
        [HttpGet("{loanAccountId}")]
        [RequiredPermission(Permissions.Loans.ViewById)]
        public async Task<IActionResult> GetLoanByIdAsync([FromRoute] string loanAccountId, CancellationToken ct)
        {
            var loan = await _loanService.GetLoanByIdAsync(loanAccountId, ct);

            return Ok(loan);
        }
    }
}
