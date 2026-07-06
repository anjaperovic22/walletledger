using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using WalletLedger.Application.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Queries.GetWalletStatistics
{
    public class GetWalletStatisticsQueryHandler : IRequestHandler<GetWalletStatisticsQuery, WalletStatisticsDTO>
    {
        private readonly IWalletRepository _walletRepository;

        public GetWalletStatisticsQueryHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<WalletStatisticsDTO> Handle(GetWalletStatisticsQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);

            if (wallet == null || wallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Nemate pristup ovom novčaniku.");

            var transactions = await _walletRepository.GetTransactionsByWalletIdAsync(request.WalletId);

            var dto = new WalletStatisticsDTO();

            dto.TotalDeposits = transactions.Where(t => t.Type == Domain.Enums.TransactionType.Deposit).Sum(t => t.Amount);
            dto.TotalWithdrawals = transactions.Where(t => t.Type == Domain.Enums.TransactionType.Withdrawal).Sum(t => t.Amount);
            dto.TotalTransfersOut = transactions.Where(t => t.Type == Domain.Enums.TransactionType.Transfer && t.WalletId == request.WalletId).Sum(t => t.Amount);

            dto.TransactionCountByType = transactions
                .GroupBy(t => t.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            dto.MonthlyInflow = transactions
                .Where(t => t.Type == Domain.Enums.TransactionType.Deposit)
                .GroupBy(t => t.Timestamp.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            dto.MonthlyOutflow = transactions
                .Where(t => t.Type == Domain.Enums.TransactionType.Withdrawal)
                .GroupBy(t => t.Timestamp.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            return dto;
        }
    }
}