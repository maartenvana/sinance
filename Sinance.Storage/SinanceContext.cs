using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sinance.Storage.Entities;
using System;

namespace Sinance.Storage
{
    public class SinanceContext : DbContext
    {
        private IUserIdProvider _userIdProvider;

        public DbSet<BankAccountEntity> BankAccounts { get; set; }

        public DbSet<CategoryEntity> Categories { get; set; }

        public DbSet<CategoryMappingEntity> CategoryMappings { get; set; }

        public DbSet<CustomReportCategoryEntity> CustomReportCategories { get; set; }

        public DbSet<CustomReportEntity> CustomReports { get; set; }

        public DbSet<ImportBankEntity> ImportBanks { get; set; }

        /// </summary>
        public DbSet<ImportMappingEntity> ImportMappings { get; set; }

        public DbSet<TransactionCategoryEntity> TransactionCategories { get; set; }

        public DbSet<TransactionEntity> Transactions { get; set; }

        public DbSet<SinanceUserEntity> Users { get; set; }

        /// <summary>
        /// Default constructors
        /// </summary>
        public SinanceContext(
            DbContextOptions<SinanceContext> options,
            IUserIdProvider userIdProvider)
            : base(options)
        {
            _userIdProvider = userIdProvider;
        }

        public void Migrate()
        {
            Database.Migrate();
        }

        public void OverwriteUserIdProvider(IUserIdProvider userIdProvider)
        {
            _userIdProvider = userIdProvider;
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
            modelBuilder.Entity<TransactionCategoryEntity>().ToTable("TransactionCategory").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImportBankEntity>().ToTable("ImportBank").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImportMappingEntity>().ToTable("ImportMapping").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<CustomReportEntity>().ToTable("CustomReport").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();
            modelBuilder.Entity<CustomReportCategoryEntity>().ToTable("CustomReportCategory").Property(x => x.Id).HasAnnotation("MySql:ValueGeneratedOnAdd", MySqlValueGenerationStrategy.IdentityColumn).ValueGeneratedOnAdd();

            modelBuilder.Entity<BankAccountEntity>().HasQueryFilter(x => x.UserId == _userIdProvider.GetCurrentUserId());
            modelBuilder.Entity<CategoryEntity>().HasQueryFilter(x => x.UserId == _userIdProvider.GetCurrentUserId());
            modelBuilder.Entity<CategoryMappingEntity>().HasQueryFilter(x => x.UserId == _userIdProvider.GetCurrentUserId());
            modelBuilder.Entity<TransactionEntity>().HasQueryFilter(x => x.UserId == _userIdProvider.GetCurrentUserId());
            modelBuilder.Entity<CustomReportEntity>().HasQueryFilter(x => x.UserId == _userIdProvider.GetCurrentUserId());

            modelBuilder.Entity<CategoryEntity>().HasIndex(x => x.ShortName).IsUnique(true);
            modelBuilder.Entity<CategoryEntity>().HasIndex(x => x.Name).IsUnique(true);

            modelBuilder.Entity<BankAccountEntity>().HasIndex(x => x.Name).IsUnique(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}