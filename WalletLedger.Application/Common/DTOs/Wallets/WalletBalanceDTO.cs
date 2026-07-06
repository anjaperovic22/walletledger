using System;
using System.Collections.Generic;
using System.Text;

namespace WalletLedger.Application.Common.DTOs.Wallets
{
    public class WalletBalanceDTO
    {
        public decimal Balance { get; set; }
        public decimal PendingAmount { get; set; }
    }
}