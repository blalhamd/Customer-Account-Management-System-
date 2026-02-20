using CAMS.API.Filters.Authentication;
using CAMS.Core.IServices;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Domains.Enums;
using CAMS.Infrastructure.constants;
using Microsoft.AspNetCore.Mvc;

namespace CAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: api/accounts/client/{clientId}
        [HttpGet("client/{clientId}")]
        [RequiredPermission(Permissions.Accounts.ViewAccountsForClient)]
        public async Task<IActionResult> GetAccountsForClient(
            [FromRoute] string clientId,
            [FromQuery] AccountQuery query,
            CancellationToken ct)
        {
            var accounts = await _accountService.GetAccountsForClientAsync(clientId, query, ct);
            return Ok(accounts);
        }

        // GET: api/accounts/{accountId}
        [HttpGet("{accountId}")]
        [RequiredPermission(Permissions.Accounts.ViewById)]
        public async Task<IActionResult> GetAccountById(
            [FromRoute] string accountId,
            CancellationToken ct)
        {
            var account = await _accountService.GetAccountByIdAsync(accountId, ct);

            if (account == null)
                return NotFound();

            return Ok(account);
        }

        // PATCH: api/accounts/{accountId}/status
        [HttpPut("{accountId}/status")]
        [RequiredPermission(Permissions.Accounts.ChangeAccountStatus)]
        public async Task<IActionResult> ChangeAccountStatus(
            [FromRoute] string accountId,
            [FromQuery] AccountStatus newStatus,
            CancellationToken ct)
        {
            await _accountService.ChangeAccountStatusAsync(accountId, newStatus, ct);
            return NoContent();
        }

        // POST: api/accounts/{accountId}/flag-signed
        [HttpPost("{accountId}/flag-signed")]
        [RequiredPermission(Permissions.Accounts.FlagAccountSigned)]
        public async Task<IActionResult> FlagAccountSigned(
            [FromRoute] string accountId,
            CancellationToken ct)
        {
            await _accountService.FlagAccountSignedAsync(accountId, ct);
            return NoContent();
        }

        // POST: api/accounts/{accountId}/close
        [HttpPost("{accountId}/close")]
        [RequiredPermission(Permissions.Accounts.CloseAccount)]
        public async Task<IActionResult> CloseAccount(
            [FromRoute] string accountId,
            CancellationToken ct)
        {
            await _accountService.CloseAccountAsync(accountId, ct);
            return NoContent();
        }

    }
}
