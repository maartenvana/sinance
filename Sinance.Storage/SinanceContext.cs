using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Sinance.Storage.Entities;

namespace Sinance.Storage
{
    public class SinanceContext : DbContext
    {
        public DbSet<BankAccountEntity> BankAccounts { get; set; }

        public DbSet<CategoryEntity> Categories { get; set; }

        public DbSet<CategoryMappingEntity> CategoryMappings { get; set; }

        public DbSet<CustomReportCategory> CustomReportCategories { get; set; }

        public DbSet<CustomReportEntity> CustomReports { get; set; }

        public DbSet<ImportBankEntity> ImportBanks { get; set; }

        /// </summary>
        public DbSet<ImportMappingEntity> ImportMappings { get; set; }

        public DbSet<TransactionCategory> TransactionCategories { get; set; }

        public DbSet<TransactionEntity> Transactions { get; set; }

        public DbSet<SinanceUserEntity> Users { get; set; }

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

            modelBuilder.Entity<BankAccountEntity>().ToTable("BankAccount").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();

            modelBuilder.Entity<SinanceUserEntity>().ToTable("Users").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<CategoryEntity>().ToTable("Category").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();

            modelBuilder.Entity<CategoryMappingEntity>().ToTable("CategoryMapping").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<TransactionEntity>().ToTable("Transaction").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<TransactionCategory>().ToTable("TransactionCategory").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImportBankEntity>().ToTable("ImportBank").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImportMappingEntity>().ToTable("ImportMapping").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<CustomReportEntity>().ToTable("CustomReport").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<CustomReportCategory>().ToTable("CustomReportCategory").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();

            base.OnModelCreating(modelBuilder);
        }
    }
}