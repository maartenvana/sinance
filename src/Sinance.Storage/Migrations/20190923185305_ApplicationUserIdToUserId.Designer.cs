﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sinance.Storage;

namespace Sinance.Storage.Migrations;

[DbContext(typeof(SinanceContext))]
[Migration("20190923185305_ApplicationUserIdToUserId")]
partial class ApplicationUserIdToUserId
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.Entity("Sinance.Domain.Entities.BankAccount", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("AccountType");

                b.Property<decimal?>("CurrentBalance");

                b.Property<bool>("Disabled");

                b.Property<string>("Name")
                    .IsRequired();

                b.Property<decimal>("StartBalance");

                b.Property<int>("UserId");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("BankAccount");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Budget", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<decimal?>("Amount");

                b.Property<int>("CategoryId");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.ToTable("Budget");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Category", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("ColorCode")
                    .IsRequired();

                b.Property<bool>("IsRegular");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(50);

                b.Property<int?>("ParentId");

                b.Property<int>("UserId");

                b.HasKey("Id");

                b.HasIndex("ParentId");

                b.HasIndex("UserId");

                b.ToTable("Category");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CategoryMapping", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("CategoryId");

                b.Property<int>("ColumnTypeId");

                b.Property<string>("MatchValue")
                    .IsRequired()
                    .HasMaxLength(200);

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.ToTable("CategoryMapping");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CustomReport", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("Name")
                    .IsRequired();

                b.Property<int>("UserId");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("CustomReport");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CustomReportCategory", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("CategoryId");

                b.Property<int>("CustomReportId");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.HasIndex("CustomReportId");

                b.ToTable("CustomReportCategory");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.ImportBank", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("Delimiter")
                    .IsRequired();

                b.Property<bool>("ImportContainsHeader");

                b.Property<string>("Name")
                    .IsRequired();

                b.HasKey("Id");

                b.ToTable("ImportBank");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.ImportMapping", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("ColumnIndex");

                b.Property<string>("ColumnName")
                    .IsRequired();

                b.Property<int>("ColumnTypeId");

                b.Property<string>("FormatValue");

                b.Property<int>("ImportBankId");

                b.HasKey("Id");

                b.HasIndex("ImportBankId");

                b.ToTable("ImportMapping");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.SinanceUser", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("Password");

                b.Property<string>("UserId");

                b.Property<string>("Username");

                b.HasKey("Id");

                b.ToTable("Users");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Transaction", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("AccountNumber")
                    .HasMaxLength(50);

                b.Property<decimal>("Amount");

                b.Property<bool>("AmountIsNegative");

                b.Property<int>("BankAccountId");

                b.Property<DateTime>("Date");

                b.Property<string>("Description")
                    .HasMaxLength(500);

                b.Property<string>("DestinationAccount")
                    .HasMaxLength(50);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(255);

                b.Property<int>("UserId");

                b.HasKey("Id");

                b.HasIndex("BankAccountId");

                b.HasIndex("UserId");

                b.ToTable("Transaction");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.TransactionCategory", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<decimal?>("Amount");

                b.Property<int>("CategoryId");

                b.Property<int>("TransactionId");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.HasIndex("TransactionId");

                b.ToTable("TransactionCategory");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.BankAccount", b =>
            {
                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Budget", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "Category")
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Category", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "ParentCategory")
                    .WithMany("ChildCategories")
                    .HasForeignKey("ParentId");

                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CategoryMapping", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "Category")
                    .WithMany("CategoryMappings")
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CustomReport", b =>
            {
                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CustomReportCategory", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "Category")
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne("Sinance.Domain.Entities.CustomReport", "CustomReport")
                    .WithMany("ReportCategories")
                    .HasForeignKey("CustomReportId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity("Sinance.Domain.Entities.ImportMapping", b =>
            {
                b.HasOne("Sinance.Domain.Entities.ImportBank", "ImportBank")
                    .WithMany("ImportMappings")
                    .HasForeignKey("ImportBankId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Transaction", b =>
            {
                b.HasOne("Sinance.Domain.Entities.BankAccount", "BankAccount")
                    .WithMany("Transactions")
                    .HasForeignKey("BankAccountId")
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity("Sinance.Domain.Entities.TransactionCategory", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "Category")
                    .WithMany("TransactionCategories")
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne("Sinance.Domain.Entities.Transaction", "Transaction")
                    .WithMany("TransactionCategories")
                    .HasForeignKey("TransactionId")
                    .OnDelete(DeleteBehavior.Restrict);
            });
#pragma warning restore 612, 618
    }
}
