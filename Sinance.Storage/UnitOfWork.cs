﻿#pragma warning disable S107 // Methods should not have too many parameters

using Sinance.Storage.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Storage
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposedValue = false;

        public IGenericRepository<BankAccountEntity> BankAccountRepository { get; }

        public IGenericRepository<CategoryMappingEntity> CategoryMappingRepository { get; }

        public IGenericRepository<CategoryEntity> CategoryRepository { get; }

        public SinanceContext Context { get; }

        public IGenericRepository<CustomReportCategoryEntity> CustomReportCategoryRepository { get; }

        public IGenericRepository<CustomReportEntity> CustomReportRepository { get; }

        public IGenericRepository<ImportBankEntity> ImportBankRepository { get; }

        public IGenericRepository<ImportMappingEntity> ImportMappingRepository { get; }

        public IGenericRepository<TransactionCategoryEntity> TransactionCategoryRepository { get; }

        public IGenericRepository<TransactionEntity> TransactionRepository { get; }

        public IGenericRepository<SinanceUserEntity> UserRepository { get; }

        public UnitOfWork(
            SinanceContext context,
            IGenericRepository<SinanceUserEntity> userRepository,
            IGenericRepository<BankAccountEntity> bankAccountRepository,
            IGenericRepository<CategoryEntity> categorieRepository,
            IGenericRepository<CategoryMappingEntity> categoryMappingRepository,
            IGenericRepository<CustomReportCategoryEntity> customReportCategorieRepository,
            IGenericRepository<CustomReportEntity> customReportRepository,
            IGenericRepository<ImportBankEntity> importBankRepository,
            IGenericRepository<ImportMappingEntity> importMappingRepository,
            IGenericRepository<TransactionCategoryEntity> transactionCategorieRepository,
            IGenericRepository<TransactionEntity> transactionRepository)
        {
            Context = context;
            UserRepository = userRepository;
            BankAccountRepository = bankAccountRepository;
            CategoryRepository = categorieRepository;
            CategoryMappingRepository = categoryMappingRepository;
            CustomReportCategoryRepository = customReportCategorieRepository;
            CustomReportRepository = customReportRepository;
            ImportBankRepository = importBankRepository;
            ImportMappingRepository = importMappingRepository;
            TransactionCategoryRepository = transactionCategorieRepository;
            TransactionRepository = transactionRepository;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<int> SaveAsync()
        {
            return await Context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Context.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}