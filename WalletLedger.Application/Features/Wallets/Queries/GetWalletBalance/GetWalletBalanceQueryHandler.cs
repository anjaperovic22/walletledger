using MediatR;
using WalletLedger.Application.Common;
using WalletLedger.Application.Common.DTOs.Wallets;
using WalletLedger.Application.Common.Helpers;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Queries.GetWalletBalance
{
    public class GetWalletBalanceQueryHandler : IRequestHandler<GetWalletBalanceQuery, WalletBalanceDTO?>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IEventStoreService _eventStoreService;

        public GetWalletBalanceQueryHandler(IWalletRepository walletRepository, IEventStoreService eventStoreService)
        {
            _walletRepository = walletRepository;
            _eventStoreService = eventStoreService;
        }

        public async Task<WalletBalanceDTO?> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);

            if (wallet == null)
                return null;

            if (wallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Nemate pristup ovom novčaniku.");

            await AutoApprovalHelper.AutoCompleteExpiredAsync(_walletRepository, _eventStoreService, request.WalletId);

            var pendingAmount = await _walletRepository.GetPendingAmountAsync(request.WalletId);

            return new WalletBalanceDTO
            {
                Balance = wallet.Balance,
                PendingAmount = pendingAmount
            };
        }
    }
}