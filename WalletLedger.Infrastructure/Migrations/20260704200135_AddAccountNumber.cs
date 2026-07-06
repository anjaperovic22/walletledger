using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Wallets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Wallets");
        }
    }
}
