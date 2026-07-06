using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletLedger.API.DTOs.Wallets;
using WalletLedger.Application.Features.Wallets.Commands.CreateWallet;
using WalletLedger.Application.Features.Wallets.Commands.DeleteWallet;
using WalletLedger.Application.Features.Wallets.Commands.DepositFunds;
using WalletLedger.Application.Features.Wallets.Commands.ExchangeFunds;
using WalletLedger.Application.Features.Wallets.Commands.ReverseTransaction;
using WalletLedger.Application.Features.Wallets.Commands.TransferFunds;
using WalletLedger.Application.Features.Wallets.Commands.WithdrawFunds;
using WalletLedger.Application.Features.Wallets.Queries.GetTransactionHistory;
using WalletLedger.Application.Features.Wallets.Queries.GetUserWallets;
using WalletLedger.Application.Features.Wallets.Queries.GetWalletAuditHistory;
using WalletLedger.Application.Features.Wallets.Queries.GetWalletBalance;
using WalletLedger.Application.Features.Wallets.Queries.GetWalletStatistics;

namespace WalletLedger.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WalletsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] string currency = "RSD")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var command = new CreateWalletCommand { UserId = userId, Currency = currency };
            var walletId = await _mediator.Send(command);

            return Ok(new { walletId });
        }


        [HttpPost("{walletId}/deposit")]
        public async Task<IActionResult> Deposit(int walletId, [FromBody] decimal amount)
        {
            var command = new DepositFundsCommand { WalletId = walletId, Amount = amount };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Wallet nije pronađen.");

            return Ok(new { message = "Uplata uspešna." });
        }

        [HttpPost("{walletId}/withdraw")]
        public async Task<IActionResult> Withdraw(int walletId, [FromBody] WithdrawRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var command = new WithdrawFundsCommand
            {
                WalletId = walletId,
                Amount = request.Amount,
                Description = request.Description,
                PayeeReference = request.PayeeReference,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Wallet nije pronađen.");

            return Ok(new { message = "Isplata uspešna." });
        }

        [HttpPost("{fromWalletId}/transfer")]
        public async Task<IActionResult> Transfer(int fromWalletId, [FromBody] TransferRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var command = new TransferFundsCommand
            {
                FromWalletId = fromWalletId,
                ToAccountNumber = request.ToAccountNumber,
                Amount = request.Amount,
                Description = request.Description,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Novčanik primaoca nije pronađen.");

            return Ok(new { message = "Transfer uspešan." });
        }

        [HttpPost("{fromWalletId}/exchange")]
        public async Task<IActionResult> Exchange(int fromWalletId, [FromBody] ExchangeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var command = new ExchangeFundsCommand
            {
                FromWalletId = fromWalletId,
                ToWalletId = request.ToWalletId,
                Amount = request.Amount,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Jedan od novčanika nije pronađen.");

            return Ok(new { message = "Konverzija uspešna." });
        }

        [HttpGet("{walletId}/balance")]
        public async Task<IActionResult> GetBalance(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _mediator.Send(new GetWalletBalanceQuery { WalletId = walletId, UserId = userId });

            if (result == null)
                return NotFound("Wallet nije pronađen.");

            return Ok(result);
        }

        [HttpGet("{walletId}/transactions")]
        public async Task<IActionResult> GetTransactions(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var transactions = await _mediator.Send(new GetTransactionHistoryQuery { WalletId = walletId, UserId = userId });
            return Ok(transactions);
        }

        [HttpGet("{walletId}/audit-history")]
        public async Task<IActionResult> GetAuditHistory(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var history = await _mediator.Send(new GetWalletAuditHistoryQuery { WalletId = walletId, UserId = userId });
            return Ok(history);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWallets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var wallets = await _mediator.Send(new GetUserWalletsQuery { UserId = userId });
            return Ok(wallets);
        }

        [HttpGet("{walletId}/statistics")]
        public async Task<IActionResult> GetStatistics(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var stats = await _mediator.Send(new GetWalletStatisticsQuery { WalletId = walletId, UserId = userId });
            return Ok(stats);
        }

        [HttpPost("transactions/{transactionId}/reverse")]
        public async Task<IActionResult> ReverseTransaction(int transactionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var command = new ReverseTransactionCommand { TransactionId = transactionId, UserId = userId };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Transakcija nije pronađena.");

            return Ok(new { message = "Transakcija stornirana." });
        }

        [HttpDelete("{walletId}")]
        public async Task<IActionResult> DeleteWallet(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var command = new DeleteWalletCommand { WalletId = walletId, UserId = userId };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Wallet nije pronađen.");

            return Ok(new { message = "Novčanik obrisan." });
        }

    }
}