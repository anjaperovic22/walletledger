using WalletLedger.Domain.Enums;

namespace WalletLedger.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;

    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? RelatedWalletId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}