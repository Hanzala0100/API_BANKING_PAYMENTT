using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_BANKING_PAYMENT.Migrations
{
    /// <inheritdoc />
    public partial class FixSuperAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1L,
                column: "PasswordHash",
                value: "$2a$11$uQuZz75pVmvWq0kMOmbSWeYtnhN9jI8IpjZLUGDGtYIbjvFzsnqqC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1L,
                column: "PasswordHash",
                value: "$2a$11$lsCO78gNZaO0h/J8Ot5Ysu0budobc5sIEWG6ctr7ldpgpJu4qroVC");
        }
    }
}
