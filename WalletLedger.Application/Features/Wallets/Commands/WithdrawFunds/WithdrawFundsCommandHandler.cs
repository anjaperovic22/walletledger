using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Enums;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Commands.WithdrawFunds
{
    public class WithdrawFundsCommandHandler : IRequestHandler<WithdrawFundsCommand, bool>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IEventStoreService _eventStoreService;

        public WithdrawFundsCommandHandler(IWalletRepository walletRepository, IEventStoreService eventStoreService)
        {
            _walletRepository = walletRepository;
            _eventStoreService = eventStoreService;
        }

        public async Task<bool> Handle(WithdrawFundsCommand request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);
            if (wallet == null)
                return false;

            if (wallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Nemate pristup ovom novčaniku.");

            if (request.Amount <= 0)
                throw new ArgumentException("Iznos mora biti veći od nule.");

            if (wallet.Balance < request.Amount)
                throw new InvalidOperationException("Nedovoljno sredstava na novčaniku.");

            wallet.Balance -= request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Withdrawal,
                Status = TransactionStatus.Completed,
                Description = request.Description ?? "Isplata sredstava",
                ReferenceNumber = request.PayeeReference ?? Guid.NewGuid().ToString(),   // koristi payee ref ako postoji
                Timestamp = DateTime.UtcNow
            };

            await _walletRepository.AddTransactionAsync(transaction);

            await _eventStoreService.SaveEventAsync(
                aggregateId: wallet.Id.ToString(),
                aggregateType: "Wallet",
                eventType: "FundsWithdrawn",
                eventData: new { Amount = request.Amount, NewBalance = wallet.Balance },
                version: wallet.CurrentVersion + 1
            );
            wallet.CurrentVersion++;

            await _walletRepository.SaveChangesAsync();

            return true;
        }
    }
}