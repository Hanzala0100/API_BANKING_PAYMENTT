using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API_BANKING_PAYMENT.Migrations
{
    /// <inheritdoc />
    public partial class updatedClientforDocs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Bank",
                keyColumn: "BankId",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Bank",
                keyColumn: "BankId",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Bank",
                keyColumn: "BankId",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Bank",
                keyColumn: "BankId",
                keyValue: 4L);

            migrationBuilder.AlterColumn<string>(
                name: "VerificationStatus",
                table: "Client",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNotes",
                table: "Client",
                type: "varchar(1000)",
                unicode: false,
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "Client",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VerifiedBy",
                table: "Client",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_VerifiedBy",
                table: "Client",
                column: "VerifiedBy");

            migrationBuilder.AddForeignKey(
                name: "client_verifiedby_foreign",
                table: "Client",
                column: "VerifiedBy",
                principalTable: "User",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "client_verifiedby_foreign",
                table: "Client");

            migrationBuilder.DropIndex(
                name: "IX_Client_VerifiedBy",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "VerificationNotes",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "Client");

            migrationBuilder.AlterColumn<string>(
                name: "VerificationStatus",
                table: "Client",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldDefaultValue: "Pending");

            migrationBuilder.InsertData(
                table: "Bank",
                columns: new[] { "BankId", "Address", "BankName", "ContactEmail", "ContactPhone", "CreatedAt" },
                values: new object[,]
                {
                    { 1L, "Madam Cama Road, Mumbai, India", "State Bank of India", "contact@sbi.co.in", "1800-123-456", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2L, "Mumbai, Maharashtra, India", "HDFC Bank", "contact@hdfcbank.com", "1800-234-567", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3L, "Mumbai, Maharashtra, India", "ICICI Bank", "contact@icicibank.com", "1800-345-678", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4L, "Mumbai, Maharashtra, India", "Axis Bank", "contact@axisbank.com", "1800-456-789", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }
    }
}
