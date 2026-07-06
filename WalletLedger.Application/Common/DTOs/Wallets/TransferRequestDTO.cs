namespace WalletLedger.API.DTOs.Wallets
{
    public record TransferRequestDTO(
        string ToAccountNumber,
        decimal Amount,
        string? Description
    );
}
