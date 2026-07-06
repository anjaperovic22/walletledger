using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using WalletLedger.Application.Common.Factories;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Enums;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Commands.ReverseTransaction
{
    public class ReverseTransactionCommandHandler : IRequestHandler<ReverseTransactionCommand, bool>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IEventStoreService _eventStoreService;

        public ReverseTransactionCommandHandler(IWalletRepository walletRepository, IEventStoreService eventStoreService)
        {
            _walletRepository = walletRepository;
            _eventStoreService = eventStoreService;
        }

        public async Task<bool> Handle(ReverseTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _walletRepository.GetTransactionByIdAsync(request.TransactionId);
            if (transaction == null)
                return false;

            var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
            if (wallet == null)
                return false;

            if (wallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Nemate pristup ovoj transakciji.");

            // Koristimo Stateless mašinu - baciće grešku ako transakcija nije u stanju Completed
            var machine = TransactionStateMachineFactory.Create(transaction.Status);
            machine.Fire(TransactionTrigger.Reverse);
            transaction.Status = machine.State;

            // Vrati sredstva - logika zavisi od tipa originalne transakcije
            switch (transaction.Type)
            {
                case TransactionType.Withdrawal:
                    wallet.Balance += transaction.Amount;
                    break;
                case TransactionType.Deposit:
                    wallet.Balance -= transaction.Amount;
                    break;
                case TransactionType.Transfer:
                    wallet.Balance += transaction.Amount;
                    if (transaction.RelatedWalletId.HasValue)
                    {
                        var relatedWallet = await _walletRepository.GetByIdAsync(transaction.RelatedWalletId.Value);
                        if (relatedWallet != null)
                        {
                            relatedWallet.Balance -= transaction.Amount;
                            relatedWallet.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException("Ovaj tip transakcije se ne može stornirati.");
            }

            wallet.UpdatedAt = DateTime.UtcNow;

            await _eventStoreService.SaveEventAsync(
                aggregateId: wallet.Id.ToString(),
                aggregateType: "Wallet",
                eventType: "TransactionReversed",
                eventData: new { TransactionId = transaction.Id, Amount = transaction.Amount, OriginalType = transaction.Type.ToString() },
                version: wallet.CurrentVersion + 1
            );
            wallet.CurrentVersion++;

            await _walletRepository.SaveChangesAsync();

            return true;
        }
    }
}