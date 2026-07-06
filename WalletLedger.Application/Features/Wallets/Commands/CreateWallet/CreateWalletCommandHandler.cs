using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Application.Common.Providers;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;

namespace WalletLedger.Application.Features.Wallets.Commands.CreateWallet
{
    public class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, int>
    {
        private readonly IWalletRepository _walletRepository;

        public CreateWalletCommandHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<int> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
        {
            var wallet = new Wallet
            {
                UserId = request.UserId,
                Currency = request.Currency,
                AccountNumber = AccountNumberGenerator.Generate(),
                Balance = 0,
                CurrentVersion = 1
            };

            await _walletRepository.AddAsync(wallet);
            await _walletRepository.SaveChangesAsync();

            return wallet.Id;
        }
    }
}