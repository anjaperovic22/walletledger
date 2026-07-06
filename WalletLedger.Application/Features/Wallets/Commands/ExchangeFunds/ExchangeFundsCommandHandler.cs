using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Application.Common.Providers;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;
using WalletLedger.Domain.Enums;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Commands.ExchangeFunds
{
    public class ExchangeFundsCommandHandler : IRequestHandler<ExchangeFundsCommand, bool>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IEventStoreService _eventStoreService;

        public ExchangeFundsCommandHandler(IWalletRepository walletRepository, IEventStoreService eventStoreService)
        {
            _walletRepository = walletRepository;
            _eventStoreService = eventStoreService;
        }

        public async Task<bool> Handle(ExchangeFundsCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Iznos mora biti veći od nule.");

            if (request.FromWalletId == request.ToWalletId)
                throw new InvalidOperationException("Izaberite dva različita novčanika.");

            var fromWallet = await _walletRepository.GetByIdAsync(request.FromWalletId);
            var toWallet = await _walletRepository.GetByIdAsync(request.ToWalletId);

            if (fromWallet == null || toWallet == null)
                return false;

            if (fromWallet.UserId != request.UserId || toWallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Možete menjati sredstva samo između svojih novčanika.");

            if (fromWallet.Currency == toWallet.Currency)
                throw new InvalidOperationException("Za konverziju su potrebne dve različite valute.");

            if (fromWallet.Balance < request.Amount)
                throw new InvalidOperationException("Nedovoljno sredstava na novčaniku.");

            var rate = ExchangeRateProvider.GetRate(fromWallet.Currency, toWallet.Currency);
            var convertedAmount = request.Amount * rate;

            fromWallet.Balance -= request.Amount;
            fromWallet.UpdatedAt = DateTime.UtcNow;

            toWallet.Balance += convertedAmount;
            toWallet.UpdatedAt = DateTime.UtcNow;

            var outgoingTransaction = new Transaction
            {
                WalletId = fromWallet.Id,
                RelatedWalletId = toWallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Exchange,
                Status = TransactionStatus.Completed,
                Description = $"Konverzija {request.Amount} {fromWallet.Currency} → {convertedAmount:F2} {toWallet.Currency} (kurs {rate:F4})",
                ReferenceNumber = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            // NOVO - Transakcija za novčanik KOJI PRIMA
            var incomingTransaction = new Transaction
            {
                WalletId = toWallet.Id,
                RelatedWalletId = fromWallet.Id,
                Amount = convertedAmount,
                Type = TransactionType.Exchange,
                Status = TransactionStatus.Completed,
                Description = $"Konverzija primljena: {convertedAmount:F2} {toWallet.Currency} (iz {request.Amount} {fromWallet.Currency}, kurs {rate:F4})",
                ReferenceNumber = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            await _walletRepository.AddTransactionAsync(outgoingTransaction);
            await _walletRepository.AddTransactionAsync(incomingTransaction);

            await _eventStoreService.SaveEventAsync(
                aggregateId: fromWallet.Id.ToString(),
                aggregateType: "Wallet",
                eventType: "CurrencyExchanged",
                eventData: new { Amount = request.Amount, ConvertedAmount = convertedAmount, FromCurrency = fromWallet.Currency, ToCurrency = toWallet.Currency, Rate = rate, ToWalletId = toWallet.Id },
                version: fromWallet.CurrentVersion + 1
            );
            fromWallet.CurrentVersion++;

            // NOVO - event i za primaoca
            await _eventStoreService.SaveEventAsync(
                aggregateId: toWallet.Id.ToString(),
                aggregateType: "Wallet",
                eventType: "CurrencyExchangeReceived",
                eventData: new { Amount = convertedAmount, FromWalletId = fromWallet.Id, FromCurrency = fromWallet.Currency, Rate = rate },
                version: toWallet.CurrentVersion + 1
            );
            toWallet.CurrentVersion++;

            await _walletRepository.SaveChangesAsync();

            return true;
        }
    }
}