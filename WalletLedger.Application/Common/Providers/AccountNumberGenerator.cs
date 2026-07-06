using System;
using System.Collections.Generic;
using System.Text;

namespace WalletLedger.Application.Common.Providers
{
    public static class AccountNumberGenerator
    {
        private static readonly Random _random = new();

        public static string Generate()
        {
            var digits = string.Concat(Enumerable.Range(0, 14).Select(_ => _random.Next(0, 10)));
            return $"RS35{digits}";
        }
    }
}