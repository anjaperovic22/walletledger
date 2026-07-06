using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace WalletLedger.Application.Features.Wallets.Commands.ReverseTransaction
{
    public class ReverseTransactionCommand : IRequest<bool>
    {
        public int TransactionId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}