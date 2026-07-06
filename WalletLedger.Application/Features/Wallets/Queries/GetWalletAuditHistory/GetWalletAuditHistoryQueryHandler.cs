using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Queries.GetWalletAuditHistory
{
    public class GetWalletAuditHistoryQueryHandler : IRequestHandler<GetWalletAuditHistoryQuery, List<StoredEvent>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IEventStoreService _eventStoreService;

        public GetWalletAuditHistoryQueryHandler(IWalletRepository walletRepository, IEventStoreService eventStoreService)
        {
            _walletRepository = walletRepository;
            _eventStoreService = eventStoreService;
        }

        public async Task<List<StoredEvent>> Handle(GetWalletAuditHistoryQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);

            if (wallet == null || wallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Nemate pristup ovom novčaniku.");

            return await _eventStoreService.GetEventsAsync(request.WalletId.ToString());
        }
    }
}