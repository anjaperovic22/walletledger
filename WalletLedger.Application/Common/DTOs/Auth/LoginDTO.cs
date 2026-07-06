namespace WalletLedger.API.DTOs.Auth
{
    public record LoginDTO(
        string Email, 
        string Password
    );
}
