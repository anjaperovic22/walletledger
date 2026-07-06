using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using WalletLedger.Application.Common.DTOs.Wallets;

namespace WalletLedger.Application.Features.Wallets.Queries.GetWalletBalance
{
    public class GetWalletBalanceQuery : IRequest<WalletBalanceDTO?>
    {
        public int WalletId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}