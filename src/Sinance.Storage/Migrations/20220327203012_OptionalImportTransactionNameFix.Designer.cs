﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sinance.Storage;

#nullable disable

namespace Sinance.Storage.Migrations;

[DbContext(typeof(SinanceContext))]
[Migration("20220327203012_OptionalImportTransactionNameFix")]
partial class OptionalImportTransactionNameFix
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "6.0.3")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.Entity("Sinance.Storage.Entities.BankAccountEntity", b =>
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
                    .HasColumnType("tinyint(1)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                b.Property<decimal>("StartBalance")
                    .HasColumnType("decimal(65,30)");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("UserId");

                b.HasIndex("Name", "UserId")
                    .IsUnique();

                b.ToTable("BankAccount", (string)null);
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CategoryEntity", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("ColorCode")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<bool>("IsRegular")
                    .HasColumnType("tinyint(1)");

                b.Property<bool>("IsStandard")
                    .HasColumnType("tinyint(1)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                b.Property<int?>("ParentId")
                    .HasColumnType("int");

                b.Property<string>("ShortName")
                    .HasMaxLength(4)
                    .HasColumnType("varchar(4)");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("ParentId");

                b.HasIndex("UserId");

                b.HasIndex("Name", "UserId")
                    .IsUnique();

                b.HasIndex("ShortName", "UserId")
                    .IsUnique();

                b.ToTable("Category", (string)null);
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CategoryMappingEntity", b =>
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
                    .HasMaxLength(200)
                    .HasColumnType("varchar(200)");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.HasIndex("UserId");

                b.ToTable("CategoryMapping", (string)null);
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CustomReportCategoryEntity", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<int>("CategoryId")
                    .HasColumnType("int");

                b.Property<int>("CustomReportId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.HasIndex("CustomReportId");

                b.ToTable("CustomReportCategory", (string)null);
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CustomReportEntity", b =>
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

                b.ToTable("CustomReport", (string)null);
            });

        modelBuilder.Entity("Sinance.Storage.Entities.ImportTransactionEntity", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("AccountNumber")
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                b.Property<decimal>("Amount")
                    .HasColumnType("decimal(65,30)");

                b.Property<int>("BankAccountId")
                    .HasColumnType("int");

                b.Property<DateTime>("Date")
                    .HasColumnType("datetime(6)");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                b.Property<string>("DestinationAccount")
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                b.Property<string>("Name")
                    .HasMaxLength(255)
                    .HasColumnType("varchar(255)");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("BankAccountId");

                b.HasIndex("UserId");

                b.ToTable("ImportTransactions", (string)null);
            });

        modelBuilder.Entity("Sinance.Storage.Entities.SinanceUserEntity", b =>
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

                b.ToTable("Users", (string)null);
            });

        modelBuilder.Entity("Sinance.Storage.Entities.TransactionEntity", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn);

                b.Property<string>("AccountNumber")
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                b.Property<decimal>("Amount")
                    .HasColumnType("decimal(65,30)");

                b.Property<int>("BankAccountId")
                    .HasColumnType("int");

                b.Property<int?>("CategoryId")
                    .HasColumnType("int");

                b.Property<DateTime>("Date")
                    .HasColumnType("datetime(6)");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("varchar(500)");

                b.Property<string>("DestinationAccount")
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                b.Property<int?>("ImportTransactionId")
                    .HasColumnType("int");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("varchar(255)");

                b.Property<int>("UserId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("BankAccountId");

                b.HasIndex("CategoryId");

                b.HasIndex("ImportTransactionId");

                b.HasIndex("UserId");

                b.ToTable("Transactions", (string)null);
            });

        modelBuilder.Entity("Sinance.Storage.Entities.BankAccountEntity", b =>
            {
                b.HasOne("Sinance.Storage.Entities.SinanceUserEntity", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("User");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CategoryEntity", b =>
            {
                b.HasOne("Sinance.Storage.Entities.CategoryEntity", "ParentCategory")
                    .WithMany("ChildCategories")
                    .HasForeignKey("ParentId");

                b.HasOne("Sinance.Storage.Entities.SinanceUserEntity", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("ParentCategory");

                b.Navigation("User");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CategoryMappingEntity", b =>
            {
                b.HasOne("Sinance.Storage.Entities.CategoryEntity", "Category")
                    .WithMany("CategoryMappings")
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Sinance.Storage.Entities.SinanceUserEntity", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Category");

                b.Navigation("User");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CustomReportCategoryEntity", b =>
            {
                b.HasOne("Sinance.Storage.Entities.CategoryEntity", "Category")
                    .WithMany()
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Sinance.Storage.Entities.CustomReportEntity", "CustomReport")
                    .WithMany("ReportCategories")
                    .HasForeignKey("CustomReportId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Category");

                b.Navigation("CustomReport");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CustomReportEntity", b =>
            {
                b.HasOne("Sinance.Storage.Entities.SinanceUserEntity", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("User");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.ImportTransactionEntity", b =>
            {
                b.HasOne("Sinance.Storage.Entities.BankAccountEntity", "BankAccount")
                    .WithMany()
                    .HasForeignKey("BankAccountId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Sinance.Storage.Entities.SinanceUserEntity", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("BankAccount");

                b.Navigation("User");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.TransactionEntity", b =>
            {
                b.HasOne("Sinance.Storage.Entities.BankAccountEntity", "BankAccount")
                    .WithMany("Transactions")
                    .HasForeignKey("BankAccountId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Sinance.Storage.Entities.CategoryEntity", "Category")
                    .WithMany()
                    .HasForeignKey("CategoryId");

                b.HasOne("Sinance.Storage.Entities.ImportTransactionEntity", "ImportTransaction")
                    .WithMany("Transactions")
                    .HasForeignKey("ImportTransactionId");

                b.HasOne("Sinance.Storage.Entities.SinanceUserEntity", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("BankAccount");

                b.Navigation("Category");

                b.Navigation("ImportTransaction");

                b.Navigation("User");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.BankAccountEntity", b =>
            {
                b.Navigation("Transactions");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CategoryEntity", b =>
            {
                b.Navigation("CategoryMappings");

                b.Navigation("ChildCategories");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.CustomReportEntity", b =>
            {
                b.Navigation("ReportCategories");
            });

        modelBuilder.Entity("Sinance.Storage.Entities.ImportTransactionEntity", b =>
            {
                b.Navigation("Transactions");
            });
#pragma warning restore 612, 618
    }
}
