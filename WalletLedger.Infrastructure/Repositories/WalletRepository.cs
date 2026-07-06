using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Entities;
using WalletLedger.Infrastructure.Data;
using WalletLedger.Domain.Enums;

namespace WalletLedger.Infrastructure.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _context;

        public WalletRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Wallet wallet)
        {
            await _context.Wallets.AddAsync(wallet);
        }

        public async Task<Wallet?> GetByIdAsync(int id)
        {
            return await _context.Wallets.FindAsync(id);
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Transaction>> GetTransactionsByWalletIdAsync(int walletId)
        {
            return await _context.Transactions
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }

        public async Task<List<Wallet>> GetByUserIdAsync(string userId)
        {
            return await _context.Wallets
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }
        public async Task<Transaction?> GetTransactionByIdAsync(int transactionId)
        {
            return await _context.Transactions.FindAsync(transactionId);
        }

        public async Task<List<Transaction>> GetPendingTransactionsAsync(int walletId)
        {
            return await _context.Transactions
                .Where(t => t.WalletId == walletId && t.Status == TransactionStatus.Pending)
                .ToListAsync();
        }

        public async Task<decimal> GetPendingAmountAsync(int walletId)
        {
            return await _context.Transactions
                .Where(t => t.WalletId == walletId && t.Status == TransactionStatus.Pending)
                .SumAsync(t => t.Amount);
        }

        public async Task<Wallet?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.Wallets.FirstOrDefaultAsync(w => w.AccountNumber == accountNumber);
        }

        public async Task<bool> HasTransactionsAsync(int walletId)
        {
            return await _context.Transactions.AnyAsync(t => t.WalletId == walletId || t.RelatedWalletId == walletId);
        }

        public Task DeleteAsync(Wallet wallet)
        {
            _context.Wallets.Remove(wallet);
            return Task.CompletedTask;
        }
    }
}