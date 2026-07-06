using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using WalletLedger.Application.Common.DTOs.Wallets;
using WalletLedger.Application.Interfaces;

namespace WalletLedger.Application.Features.Wallets.Queries.GetUserWallets
{
    public class GetUserWalletsQueryHandler : IRequestHandler<GetUserWalletsQuery, List<WalletDTO>>
    {
        private readonly IWalletRepository _walletRepository;

        public GetUserWalletsQueryHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<List<WalletDTO>> Handle(GetUserWalletsQuery request, CancellationToken cancellationToken)
        {
            var wallets = await _walletRepository.GetByUserIdAsync(request.UserId);

            return wallets.Select(w => new WalletDTO
            {
                Id = w.Id,
                Currency = w.Currency,
                AccountNumber = w.AccountNumber,
                Balance = w.Balance,
                CreatedAt = w.CreatedAt
            }).ToList();
        }
    }
}