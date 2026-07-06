namespace WalletLedger.Application.Common.DTOs.Wallets
{
    public class WalletDTO
    {
        public int Id { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}