using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace WalletLedger.Application.Features.Wallets.Commands.ExchangeFunds
{
    public class ExchangeFundsCommand : IRequest<bool>
    {
        public int FromWalletId { get; set; }
        public int ToWalletId { get; set; }
        public decimal Amount { get; set; }
        public string UserId { get; set; } = string.Empty; // iz JWT-a, za proveru vlasnistva
    }
}