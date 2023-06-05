#pragma warning disable S107 // Methods should not have too many parameters

using Sinance.Storage.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Storage;

public class UnitOfWork : IUnitOfWork
{
    public IGenericRepository<BankAccountEntity> BankAccountRepository { get; }

    public IGenericRepository<CategoryMappingEntity> CategoryMappingRepository { get; }

    public IGenericRepository<CategoryEntity> CategoryRepository { get; }

    public SinanceContext Context { get; }

    public IGenericRepository<CustomReportCategoryEntity> CustomReportCategoryRepository { get; }

    public IGenericRepository<CustomReportEntity> CustomReportRepository { get; }

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
        TransactionCategoryRepository = transactionCategorieRepository;
        TransactionRepository = transactionRepository;
    }

    public async Task<int> SaveAsync()
    {
        return await Context.SaveChangesAsync();
    }
}