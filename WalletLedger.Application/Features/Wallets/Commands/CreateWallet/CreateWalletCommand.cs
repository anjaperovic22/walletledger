using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace WalletLedger.Application.Features.Wallets.Commands.CreateWallet
{
    public class CreateWalletCommand : IRequest<int>
    {
        public string UserId { get; set; } = string.Empty;
        public string Currency { get; set; } = "RSD";
    }
}