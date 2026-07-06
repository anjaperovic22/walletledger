namespace WalletLedger.API.DTOs.Wallets
{
    public record WithdrawRequestDTO(
        decimal Amount, 
        string? Description, 
        string? PayeeReference
    );

}
