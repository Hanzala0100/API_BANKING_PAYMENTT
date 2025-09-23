using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_BANKING_PAYMENT.Migrations
{
    /// <inheritdoc />
    public partial class PendingChangesCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1L,
                column: "PasswordHash",
                value: "$2a$11$jHouR8b8dzPXF6gipSngJungngAfK2s./8WMSWUpor0Zs1h9iDTnS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1L,
                column: "PasswordHash",
                value: "$2a$11$uQuZz75pVmvWq0kMOmbSWeYtnhN9jI8IpjZLUGDGtYIbjvFzsnqqC");
        }
    }
}
