using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Application.Common.DTOs.Wallets;
using WalletLedger.Domain.Entities;

namespace WalletLedger.Application.Features.Wallets.Queries.GetUserWallets
{
    public class GetUserWalletsQuery : IRequest<List<WalletDTO>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}