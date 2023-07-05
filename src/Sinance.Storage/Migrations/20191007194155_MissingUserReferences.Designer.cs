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
[Migration("20191007194155_MissingUserReferences")]
partial class MissingUserReferences
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "3.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.Entity("Sinance.Domain.Entities.BankAccount", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("AccountType")
                    .HasColumnType("int");

                b.Property<decimal?>("CurrentBalance")
                    .HasColumnType("decimal(65,30)");

                b.Property<bool>("Disabled")
                    .HasColumnType("bit");

                b.Property<bool>("IncludeInProfitLossGraph")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<decimal>("StartBalance")
                    .HasColumnType("decimal(65,30)");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("BankAccount");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Category", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("ColorCode")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<bool>("IsRegular")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50);

                b.Property<int?>("ParentId")
                    .HasColumnType("int");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("ParentId");

                b.HasIndex("UserId");

                b.ToTable("Category");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CategoryMapping", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("CategoryId")
                    .HasColumnType("int");

                b.Property<int>("ColumnTypeId")
                    .HasColumnType("int");

                b.Property<string>("MatchValue")
                    .IsRequired()
                    .HasColumnType("varchar(200)")
                    .HasMaxLength(200);

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.HasIndex("UserId");

                b.ToTable("CategoryMapping");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CustomReport", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("CustomReport");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CustomReportCategory", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("CategoryId")
                    .HasColumnType("int");

                b.Property<int>("CustomReportId")
                    .HasColumnType("int");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.HasIndex("CustomReportId");

                b.HasIndex("UserId");

                b.ToTable("CustomReportCategory");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.ImportBank", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("Delimiter")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<bool>("ImportContainsHeader")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.ToTable("ImportBank");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.ImportMapping", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("ColumnIndex")
                    .HasColumnType("int");

                b.Property<string>("ColumnName")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<int>("ColumnTypeId")
                    .HasColumnType("int");

                b.Property<string>("FormatValue")
                    .HasColumnType("longtext");

                b.Property<int>("ImportBankId")
                    .HasColumnType("int");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("ImportBankId");

                b.HasIndex("UserId");

                b.ToTable("ImportMapping");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.SinanceUser", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("Password")
                    .HasColumnType("longtext");

                b.Property<string>("Username")
                    .HasColumnType("longtext");

                b.HasKey("Id");

                b.ToTable("Users");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Transaction", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("AccountNumber")
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50);

                b.Property<decimal>("Amount")
                    .HasColumnType("decimal(65,30)");

                b.Property<bool>("AmountIsNegative")
                    .HasColumnType("bit");

                b.Property<int>("BankAccountId")
                    .HasColumnType("int");

                b.Property<DateTime>("Date")
                    .HasColumnType("datetime(6)");

                b.Property<string>("Description")
                    .HasColumnType("varchar(500)")
                    .HasMaxLength(500);

                b.Property<string>("DestinationAccount")
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50);

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasMaxLength(255);

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("BankAccountId");

                b.HasIndex("UserId");

                b.ToTable("Transaction");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.TransactionCategory", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<decimal?>("Amount")
                    .HasColumnType("decimal(65,30)");

                b.Property<int>("CategoryId")
                    .HasColumnType("int");

                b.Property<int>("TransactionId")
                    .HasColumnType("int");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.HasIndex("TransactionId");

                b.HasIndex("UserId");

                b.ToTable("TransactionCategory");
            });

        modelBuilder.Entity("Sinance.Domain.Entities.BankAccount", b =>
            {
                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Category", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "ParentCategory")
                    .WithMany("ChildCategories")
                    .HasForeignKey("ParentId");

                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CategoryMapping", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "Category")
                    .WithMany("CategoryMappings")
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CustomReport", b =>
            {
                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

        modelBuilder.Entity("Sinance.Domain.Entities.CustomReportCategory", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "Category")
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Sinance.Domain.Entities.CustomReport", "CustomReport")
                    .WithMany("ReportCategories")
                    .HasForeignKey("CustomReportId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

        modelBuilder.Entity("Sinance.Domain.Entities.ImportBank", b =>
            {
                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

        modelBuilder.Entity("Sinance.Domain.Entities.ImportMapping", b =>
            {
                b.HasOne("Sinance.Domain.Entities.ImportBank", "ImportBank")
                    .WithMany("ImportMappings")
                    .HasForeignKey("ImportBankId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

        modelBuilder.Entity("Sinance.Domain.Entities.Transaction", b =>
            {
                b.HasOne("Sinance.Domain.Entities.BankAccount", "BankAccount")
                    .WithMany("Transactions")
                    .HasForeignKey("BankAccountId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });

        modelBuilder.Entity("Sinance.Domain.Entities.TransactionCategory", b =>
            {
                b.HasOne("Sinance.Domain.Entities.Category", "Category")
                    .WithMany("TransactionCategories")
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Sinance.Domain.Entities.Transaction", "Transaction")
                    .WithMany("TransactionCategories")
                    .HasForeignKey("TransactionId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Sinance.Domain.Entities.SinanceUser", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });
#pragma warning restore 612, 618
    }
}
