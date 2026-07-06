namespace WalletLedger.Application.Common.DTOs.Wallets
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public int? RelatedWalletId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}