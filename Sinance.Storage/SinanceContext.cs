﻿using Sinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Sinance.Storage
{
    public class SinanceContext : DbContext
    {
        public DbSet<BankAccount> BankAccounts { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<CategoryMapping> CategoryMappings { get; set; }

        public DbSet<CustomReportCategory> CustomReportCategories { get; set; }

        public DbSet<CustomReport> CustomReports { get; set; }

        public DbSet<ImportBank> ImportBanks { get; set; }

        /// </summary>
        public DbSet<ImportMapping> ImportMappings { get; set; }

        public DbSet<TransactionCategory> TransactionCategories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<SinanceUser> Users { get; set; }

        /// <summary>
        /// Default constructors
        /// </summary>
        public SinanceContext(DbContextOptions options)
            : base(options)
        {
        }

        public void Migrate()
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<BankAccount>().ToTable("BankAccount").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();

            modelBuilder.Entity<SinanceUser>().ToTable("Users").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<Category>().ToTable("Category").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();

            modelBuilder.Entity<CategoryMapping>().ToTable("CategoryMapping").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<Transaction>().ToTable("Transaction").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<TransactionCategory>().ToTable("TransactionCategory").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImportBank>().ToTable("ImportBank").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImportMapping>().ToTable("ImportMapping").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<CustomReport>().ToTable("CustomReport").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<CustomReportCategory>().ToTable("CustomReportCategory").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();

            base.OnModelCreating(modelBuilder);
        }
    }
}