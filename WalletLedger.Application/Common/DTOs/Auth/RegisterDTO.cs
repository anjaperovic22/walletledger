namespace WalletLedger.API.DTOs.Auth
{
    public record RegisterDTO(
        string Email, 
        string Password, 
        string FirstName, 
        string LastName
    );
}
