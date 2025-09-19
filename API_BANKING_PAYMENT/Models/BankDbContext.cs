using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Models;

public partial class BankDbContext : DbContext
{
    
    public BankDbContext()
    {
    }

    public BankDbContext(DbContextOptions<BankDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<Beneficiary> Beneficiaries { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<SalaryDisbursement> SalaryDisbursements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.BankId).HasName("bank_bankid_primary");

            entity.ToTable("Bank");

            entity.Property(e => e.BankId).ValueGeneratedNever();
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.BankName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.HasKey(e => e.BeneficiaryId).HasName("beneficiary_beneficiaryid_primary");

            entity.ToTable("Beneficiary");

            entity.Property(e => e.BeneficiaryId).ValueGeneratedNever();
            entity.Property(e => e.BankName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Ifsccode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("IFSCCode");

            entity.HasOne(d => d.Client).WithMany(p => p.Beneficiaries)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("beneficiary_clientid_foreign");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("client_clientid_primary");

            entity.ToTable("Client");

            entity.HasIndex(e => e.RegisterationNumber, "client_registerationnumber_unique").IsUnique();

            entity.Property(e => e.ClientId).ValueGeneratedNever();
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ClientName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.RegisterationNumber)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.VerificationStatus).HasMaxLength(255);

            entity.HasOne(d => d.Bank).WithMany(p => p.Clients)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("client_bankid_foreign");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("document_documentid_primary");

            entity.ToTable("Document");

            entity.Property(e => e.DocumentId).ValueGeneratedNever();
            entity.Property(e => e.DocType)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FileUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UploadedAt).HasColumnType("datetime");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_uploadedby_foreign");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("employee_employeeid_primary");

            entity.ToTable("Employee");

            entity.HasIndex(e => e.Email, "employee_email_unique").IsUnique();

            entity.Property(e => e.EmployeeId).ValueGeneratedNever();
            entity.Property(e => e.BankName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Ifsccode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("IFSCcode");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SalaryAmount).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Client).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("employee_clientid_foreign");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payment_paymentid_primary");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("payment_approvedby_foreign");

            entity.HasOne(d => d.Beneficiary).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BeneficiaryId)
                .HasConstraintName("payment_beneficiaryid_foreign");

            entity.HasOne(d => d.Client).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("payment_clientid_foreign");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("report_reportid_primary");

            entity.ToTable("Report");

            entity.Property(e => e.ReportId).ValueGeneratedNever();
            entity.Property(e => e.FileUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.GeneratedAt).HasColumnType("datetime");
            entity.Property(e => e.ReportType)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.GeneratedByNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.GeneratedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("report_generatedby_foreign");
        });

        modelBuilder.Entity<SalaryDisbursement>(entity =>
        {
            entity.HasKey(e => e.SalaryId).HasName("salarydisbursement_salaryid_primary");

            entity.ToTable("SalaryDisbursement");

            entity.Property(e => e.SalaryId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DisbursementDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Client).WithMany(p => p.SalaryDisbursements)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("salarydisbursement_clientid_foreign");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryDisbursements)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("salarydisbursement_employeeid_foreign");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_userid_primary");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "user_email_unique").IsUnique();

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role).HasMaxLength(255);

            entity.HasOne(d => d.Bank).WithMany(p => p.Users)
                .HasForeignKey(d => d.BankId)
                .HasConstraintName("user_bankid_foreign");

            entity.HasOne(d => d.Client).WithMany(p => p.Users)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("user_clientid_foreign");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
