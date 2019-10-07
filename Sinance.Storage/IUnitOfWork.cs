using Sinance.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Storage
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<BankAccount> BankAccountRepository { get; }
        IGenericRepository<CategoryMapping> CategoryMappingRepository { get; }
        IGenericRepository<Category> CategoryRepository { get; }
        SinanceContext Context { get; }
        IGenericRepository<CustomReportCategory> CustomReportCategoryRepository { get; }
        IGenericRepository<CustomReport> CustomReportRepository { get; }
        IGenericRepository<ImportBank> ImportBankRepository { get; }
        IGenericRepository<ImportMapping> ImportMappingRepository { get; }
        IGenericRepository<TransactionCategory> TransactionCategoryRepository { get; }
        IGenericRepository<Transaction> TransactionRepository { get; }
        IGenericRepository<SinanceUser> UserRepository { get; }

        Task<int> SaveAsync();
    }
}