using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using WalletLedger.Application.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Commands.DeleteWallet
{
    public class DeleteWalletCommandHandler : IRequestHandler<DeleteWalletCommand, bool>
    {
        private readonly IWalletRepository _walletRepository;

        public DeleteWalletCommandHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<bool> Handle(DeleteWalletCommand request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);
            if (wallet == null)
                return false;

            if (wallet.UserId != request.UserId)
                throw new UnauthorizedAccessException("Nemate pristup ovom novčaniku.");

            if (wallet.Balance != 0)
                throw new InvalidOperationException("Novčanik mora imati stanje 0 da bi bio obrisan.");

            var hasTransactions = await _walletRepository.HasTransactionsAsync(request.WalletId);
            if (hasTransactions)
                throw new InvalidOperationException("Novčanik sa istorijom transakcija ne može biti obrisan.");

            await _walletRepository.DeleteAsync(wallet);
            await _walletRepository.SaveChangesAsync();

            return true;
        }
    }
}