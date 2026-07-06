using System;
using System.Collections.Generic;
using System.Text;
using WalletLedger.Application.Common.Factories;
using WalletLedger.Application.Interfaces;
using WalletLedger.Domain.Enums;
using WalletLedger.Domain.Interfaces;

namespace WalletLedger.Application.Common.Helpers
{
    public static class AutoApprovalHelper
    {
        private static readonly TimeSpan ApprovalThreshold = TimeSpan.FromSeconds(30);

        public static async Task AutoCompleteExpiredAsync(
            IWalletRepository walletRepository,
            IEventStoreService eventStoreService,
            int walletId)
        {
            var pending = await walletRepository.GetPendingTransactionsAsync(walletId);
            var expired = pending.Where(t => DateTime.UtcNow - t.Timestamp >= ApprovalThreshold).ToList();

            if (expired.Count == 0)
                return;

            var wallet = await walletRepository.GetByIdAsync(walletId);
            if (wallet == null) return;

            foreach (var transaction in expired)
            {
                var machine = TransactionStateMachineFactory.Create(transaction.Status);
                machine.Fire(TransactionTrigger.Complete);
                transaction.Status = machine.State;

                await eventStoreService.SaveEventAsync(
                    aggregateId: walletId.ToString(),
                    aggregateType: "Wallet",
                    eventType: "TransactionAutoApproved",
                    eventData: new { TransactionId = transaction.Id, transaction.Amount },
                    version: wallet.CurrentVersion + 1
                );
                wallet.CurrentVersion++;
            }

            await walletRepository.SaveChangesAsync();
        }
    }
}