using System;
using System.Collections.Generic;
using System.Text;

using WalletLedger.Domain.Entities;

namespace WalletLedger.Application.Interfaces
{
    public interface IWalletRepository
    {
        Task AddAsync(Wallet wallet);
        Task<Wallet?> GetByIdAsync(int id);
        Task AddTransactionAsync(Transaction transaction);
        Task SaveChangesAsync();
        Task<List<Transaction>> GetTransactionsByWalletIdAsync(int walletId);
        Task<List<Wallet>> GetByUserIdAsync(string userId);
        Task<Transaction?> GetTransactionByIdAsync(int transactionId);
        Task<List<Transaction>> GetPendingTransactionsAsync(int walletId);
        Task<decimal> GetPendingAmountAsync(int walletId);
        Task<Wallet?> GetByAccountNumberAsync(string accountNumber);
        Task<bool> HasTransactionsAsync(int walletId);
        Task DeleteAsync(Wallet wallet);

    }
}