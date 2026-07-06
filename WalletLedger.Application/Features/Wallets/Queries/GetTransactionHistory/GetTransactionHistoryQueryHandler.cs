using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Application.Common.DTOs.Wallets;
using WalletLedger.Application.Common.Helpers;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Queries.GetTransactionHistory
{
    public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, List<TransactionDTO>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IEventStoreService _eventStoreService;

        public GetTransactionHistoryQueryHandler(IWalletRepository walletRepository, IEventStoreService eventStoreService)
        {
            _walletRepository = walletRepository;
            _eventStoreService = eventStoreService;
        }

        public async Task<List<TransactionDTO>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);

            if (wallet == null || wallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Nemate pristup ovom novčaniku.");

            await AutoApprovalHelper.AutoCompleteExpiredAsync(_walletRepository, _eventStoreService, request.WalletId);

            var transactions = await _walletRepository.GetTransactionsByWalletIdAsync(request.WalletId);

            return transactions.Select(t => new TransactionDTO
            {
                Id = t.Id,
                WalletId = t.WalletId,
                RelatedWalletId = t.RelatedWalletId,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Status = t.Status.ToString(),
                Description = t.Description,
                ReferenceNumber = t.ReferenceNumber,
                Timestamp = t.Timestamp
            }).ToList();
        }
    }
}