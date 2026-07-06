namespace WalletLedger.API.DTOs.Wallets
{
    public record ExchangeRequest(
        int ToWalletId, 
        decimal Amount
    );
}
