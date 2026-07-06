using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace WalletLedger.Application.Features.Wallets.Commands.TransferFunds
{
    public class TransferFundsCommand : IRequest<bool>
    {
        public int FromWalletId { get; set; }
        public string ToAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}