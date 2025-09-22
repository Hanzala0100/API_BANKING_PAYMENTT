using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_BANKING_PAYMENT.Migrations
{
    /// <inheritdoc />
    public partial class MakeIdsAutoIncrement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    BankId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    ContactEmail = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("bank_bankid_primary", x => x.BankId);
                });

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    ClientId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<long>(type: "bigint", nullable: false),
                    ClientName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    RegisterationNumber = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    VerificationStatus = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("client_clientid_primary", x => x.ClientId);
                    table.ForeignKey(
                        name: "client_bankid_foreign",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "BankId");
                });

            migrationBuilder.CreateTable(
                name: "Beneficiary",
                columns: table => new
                {
                    BeneficiaryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    FullName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    BankName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    IFSCCode = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("beneficiary_beneficiaryid_primary", x => x.BeneficiaryId);
                    table.ForeignKey(
                        name: "beneficiary_clientid_foreign",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId");
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    FullName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    BankName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    IFSCcode = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    SalaryAmount = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("employee_employeeid_primary", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "employee_clientid_foreign",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BankId = table.Column<long>(type: "bigint", nullable: true),
                    ClientId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_userid_primary", x => x.UserId);
                    table.ForeignKey(
                        name: "user_bankid_foreign",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "BankId");
                    table.ForeignKey(
                        name: "user_clientid_foreign",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId");
                });

            migrationBuilder.CreateTable(
                name: "SalaryDisbursement",
                columns: table => new
                {
                    SalaryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "Pending"),
                    DisbursementDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("salarydisbursement_salaryid_primary", x => x.SalaryId);
                    table.ForeignKey(
                        name: "salarydisbursement_clientid_foreign",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId");
                    table.ForeignKey(
                        name: "salarydisbursement_employeeid_foreign",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    DocumentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadedBy = table.Column<long>(type: "bigint", nullable: false),
                    ClientId = table.Column<long>(type: "bigint", nullable: true),
                    BankId = table.Column<long>(type: "bigint", nullable: false),
                    FileName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    DocType = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("document_documentid_primary", x => x.DocumentId);
                    table.ForeignKey(
                        name: "document_uploadedby_foreign",
                        column: x => x.UploadedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PaymentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<long>(type: "bigint", nullable: true),
                    BeneficiaryId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "Pending"),
                    ApprovedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("payment_paymentid_primary", x => x.PaymentId);
                    table.ForeignKey(
                        name: "payment_approvedby_foreign",
                        column: x => x.ApprovedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "payment_beneficiaryid_foreign",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiary",
                        principalColumn: "BeneficiaryId");
                    table.ForeignKey(
                        name: "payment_clientid_foreign",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "ClientId");
                });

            migrationBuilder.CreateTable(
                name: "Report",
                columns: table => new
                {
                    ReportId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedBy = table.Column<long>(type: "bigint", nullable: false),
                    ReportType = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    FileUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("report_reportid_primary", x => x.ReportId);
                    table.ForeignKey(
                        name: "report_generatedby_foreign",
                        column: x => x.GeneratedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiary_ClientId",
                table: "Beneficiary",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "client_registerationnumber_unique",
                table: "Client",
                column: "RegisterationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_BankId",
                table: "Client",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_UploadedBy",
                table: "Document",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "employee_email_unique",
                table: "Employee",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_ClientId",
                table: "Employee",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ApprovedBy",
                table: "Payment",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_BeneficiaryId",
                table: "Payment",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ClientId",
                table: "Payment",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Report_GeneratedBy",
                table: "Report",
                column: "GeneratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDisbursement_ClientId",
                table: "SalaryDisbursement",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDisbursement_EmployeeId",
                table: "SalaryDisbursement",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_User_BankId",
                table: "User",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_User_ClientId",
                table: "User",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "user_email_unique",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Report");

            migrationBuilder.DropTable(
                name: "SalaryDisbursement");

            migrationBuilder.DropTable(
                name: "Beneficiary");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Bank");
        }
    }
}
