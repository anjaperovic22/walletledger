using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using WalletLedger.Domain.Entities;

namespace WalletLedger.Application.Features.Wallets.Queries.GetWalletAuditHistory
{
    public class GetWalletAuditHistoryQuery : IRequest<List<StoredEvent>>
    {
        public int WalletId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}