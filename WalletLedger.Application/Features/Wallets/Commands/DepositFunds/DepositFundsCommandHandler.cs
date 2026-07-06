using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Enums;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Commands.DepositFunds
{
    public class DepositFundsCommandHandler : IRequestHandler<DepositFundsCommand, bool>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IEventStoreService _eventStoreService;

        public DepositFundsCommandHandler(IWalletRepository walletRepository, IEventStoreService eventStoreService)
        {
            _walletRepository = walletRepository;
            _eventStoreService = eventStoreService;
        }

        public async Task<bool> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);
            if (wallet == null)
                return false;

            if (request.Amount <= 0)
                throw new ArgumentException("Iznos mora biti veći od nule.");

            wallet.Balance += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                Description = request.Description ?? "Uplata sredstava",
                ReferenceNumber = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            await _walletRepository.AddTransactionAsync(transaction);

            await _eventStoreService.SaveEventAsync(
                aggregateId: wallet.Id.ToString(),
                aggregateType: "Wallet",
                eventType: "FundsDeposited",
                eventData: new { Amount = request.Amount, NewBalance = wallet.Balance },
                version: wallet.CurrentVersion + 1
            );
            wallet.CurrentVersion++;

            await _walletRepository.SaveChangesAsync();

            return true;
        }
    }
}