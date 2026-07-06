using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace WalletLedger.Application.Features.Wallets.Queries.GetWalletStatistics
{
    public class GetWalletStatisticsQuery : IRequest<WalletStatisticsDTO>
    {
        public int WalletId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class WalletStatisticsDTO
    {
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal TotalTransfersOut { get; set; }
        public decimal TotalTransfersIn { get; set; }
        public Dictionary<string, int> TransactionCountByType { get; set; } = new();
        public Dictionary<string, decimal> MonthlyInflow { get; set; } = new();
        public Dictionary<string, decimal> MonthlyOutflow { get; set; } = new();
    }
}