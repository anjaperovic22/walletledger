using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace WalletLedger.Application.Features.Wallets.Commands.DeleteWallet
{
    public class DeleteWalletCommand : IRequest<bool>
    {
        public int WalletId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}