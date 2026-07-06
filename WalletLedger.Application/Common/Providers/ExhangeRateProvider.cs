using System;
using System.Collections.Generic;
using System.Text;

namespace WalletLedger.Application.Common.Providers
{
    public static class ExchangeRateProvider
    {
        private static readonly Dictionary<(string From, string To), decimal> _rates = new()
        {
            // EUR <-> RSD
            { ("EUR", "RSD"), 117.0m },
            { ("RSD", "EUR"), 1 / 117.0m },

            // USD <-> RSD
            { ("USD", "RSD"), 108.0m },
            { ("RSD", "USD"), 1 / 108.0m },

            // BAM <-> RSD
            { ("BAM", "RSD"), 59.8m },
            { ("RSD", "BAM"), 1 / 59.8m },

            // EUR <-> USD
            { ("EUR", "USD"), 1.08m },
            { ("USD", "EUR"), 1 / 1.08m },

            // EUR <-> BAM
            { ("EUR", "BAM"), 1.95583m },
            { ("BAM", "EUR"), 1 / 1.95583m },

            // USD <-> BAM
            { ("USD", "BAM"), 1.81m },
            { ("BAM", "USD"), 1 / 1.81m }
        };

        public static decimal GetRate(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return 1m;

            if (_rates.TryGetValue((fromCurrency, toCurrency), out var rate))
                return rate;

            throw new InvalidOperationException($"Kurs za {fromCurrency} → {toCurrency} nije definisan.");
        }
    }
}