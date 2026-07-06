using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using WalletLedger.Application.Common.DTOs.Wallets;

namespace WalletLedger.Application.Features.Wallets.Queries.GetTransactionHistory
{
    public class GetTransactionHistoryQuery : IRequest<List<TransactionDTO>>
    {
        public int WalletId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}