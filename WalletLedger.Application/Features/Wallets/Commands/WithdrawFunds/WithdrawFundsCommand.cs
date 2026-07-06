using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace WalletLedger.Application.Features.Wallets.Commands.WithdrawFunds
{
    public class WithdrawFundsCommand : IRequest<bool>
    {
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? PayeeReference { get; set; }   // NOVO - npr. broj računa za struju
    }
}