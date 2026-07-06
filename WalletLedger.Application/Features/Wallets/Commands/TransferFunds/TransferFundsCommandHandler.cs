using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Application.Common.Factories;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Enums;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Commands.TransferFunds
{
    public class TransferFundsCommandHandler : IRequestHandler<TransferFundsCommand, bool>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IEventStoreService _eventStoreService;

        public TransferFundsCommandHandler(IWalletRepository walletRepository, IEventStoreService eventStoreService)
        {
            _walletRepository = walletRepository;
            _eventStoreService = eventStoreService;
        }

        public async Task<bool> Handle(TransferFundsCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Iznos mora biti veći od nule.");

            var fromWallet = await _walletRepository.GetByIdAsync(request.FromWalletId);
            var toWallet = await _walletRepository.GetByAccountNumberAsync(request.ToAccountNumber);

            if (fromWallet == null || toWallet == null)
                return false;

            if (fromWallet.Id == toWallet.Id)
                throw new InvalidOperationException("Ne možete prebaciti sredstva na isti novčanik.");

            if (fromWallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Možete slati sredstva samo sa svog novčanika.");

            if (fromWallet.Currency != toWallet.Currency)
                throw new InvalidOperationException("Transfer je moguć samo između novčanika iste valute.");

            var transaction = new Transaction
            {
                WalletId = fromWallet.Id,
                RelatedWalletId = toWallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Pending,
                Description = request.Description ?? "Transfer sredstava",
                ReferenceNumber = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            var machine = TransactionStateMachineFactory.Create(transaction.Status);

            if (fromWallet.Balance < request.Amount)
            {
                machine.Fire(TransactionTrigger.Fail);
                transaction.Status = machine.State;

                await _walletRepository.AddTransactionAsync(transaction);

                await _eventStoreService.SaveEventAsync(
                    aggregateId: fromWallet.Id.ToString(),
                    aggregateType: "Wallet",
                    eventType: "TransferFailed",
                    eventData: new
                    {
                        Amount = request.Amount,
                        ToWalletId = toWallet.Id,
                        Reason = "Nedovoljno sredstava"
                    },
                    version: fromWallet.CurrentVersion + 1
                );
                fromWallet.CurrentVersion++;

                await _walletRepository.SaveChangesAsync();

                await _walletRepository.SaveChangesAsync();

                throw new InvalidOperationException("Nedovoljno sredstava na novčaniku.");
            }

            fromWallet.Balance -= request.Amount;
            fromWallet.UpdatedAt = DateTime.UtcNow;

            toWallet.Balance += request.Amount;
            toWallet.UpdatedAt = DateTime.UtcNow;

            machine.Fire(TransactionTrigger.Complete);
            transaction.Status = machine.State;

            var incomingTransaction = new Transaction
            {
                WalletId = toWallet.Id,
                RelatedWalletId = fromWallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,   // primalac odmah vidi Completed, ne prolazi kroz Pending
                Description = $"Primljen transfer" + (request.Description != null ? $": {request.Description}" : ""),
                ReferenceNumber = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            await _walletRepository.AddTransactionAsync(transaction);
            await _walletRepository.AddTransactionAsync(incomingTransaction);   // NOVO

            await _eventStoreService.SaveEventAsync(
                aggregateId: fromWallet.Id.ToString(),
                aggregateType: "Wallet",
                eventType: "TransferCompleted",
                eventData: new { Amount = request.Amount, FromWalletId = fromWallet.Id, ToWalletId = toWallet.Id },
                version: fromWallet.CurrentVersion + 1
            );
            fromWallet.CurrentVersion++;

            await _eventStoreService.SaveEventAsync(
                aggregateId: toWallet.Id.ToString(),
                aggregateType: "Wallet",
                eventType: "TransferReceived",
                eventData: new { Amount = request.Amount, FromWalletId = fromWallet.Id },
                version: toWallet.CurrentVersion + 1
            );
            toWallet.CurrentVersion++;

            await _walletRepository.SaveChangesAsync();

            return true;
        }
    }
}
