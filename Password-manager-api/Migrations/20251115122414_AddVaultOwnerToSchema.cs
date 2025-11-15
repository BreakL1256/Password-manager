using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Password_manager_api.Migrations
{
    /// <inheritdoc />
    public partial class AddVaultOwnerToSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VaultOwnerId",
                table: "VaultBackups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaultOwnerId",
                table: "VaultBackups");
        }
    }
}
