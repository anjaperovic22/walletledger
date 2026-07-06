using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace WalletLedger.Application.Features.Wallets.Commands.DepositFunds
{
    public class DepositFundsCommand : IRequest<bool>
    {
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}