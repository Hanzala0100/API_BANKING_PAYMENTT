using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_BANKING_PAYMENT.Migrations
{
    /// <inheritdoc />
    public partial class SeedSuperAdminUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "BankId", "ClientId", "CreatedAt", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { 1L, null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "superAdmin@gmail.com", "SuperAdmin", "$2a$11$lsCO78gNZaO0h/J8Ot5Ysu0budobc5sIEWG6ctr7ldpgpJu4qroVC", "SuperAdmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1L);
        }
    }
}
