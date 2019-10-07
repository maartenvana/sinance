using Sinance.Domain.Entities;
using System.Threading.Tasks;

namespace Sinance.Storage
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposedValue = false;

        public IGenericRepository<BankAccount> BankAccountRepository { get; }

        public IGenericRepository<CategoryMapping> CategoryMappingRepository { get; }

        public IGenericRepository<Category> CategoryRepository { get; }

        public SinanceContext Context { get; }

        public IGenericRepository<CustomReportCategory> CustomReportCategoryRepository { get; }

        public IGenericRepository<CustomReport> CustomReportRepository { get; }

        public IGenericRepository<ImportBank> ImportBankRepository { get; }

        public IGenericRepository<ImportMapping> ImportMappingRepository { get; }

        public IGenericRepository<TransactionCategory> TransactionCategoryRepository { get; }

        public IGenericRepository<Transaction> TransactionRepository { get; }

        public IGenericRepository<SinanceUser> UserRepository { get; }

        public UnitOfWork(
            SinanceContext context,
            IGenericRepository<SinanceUser> userRepository,
            IGenericRepository<BankAccount> bankAccountRepository,
            IGenericRepository<Category> categorieRepository,
            IGenericRepository<CategoryMapping> categoryMappingRepository,
            IGenericRepository<CustomReportCategory> customReportCategorieRepository,
            IGenericRepository<CustomReport> customReportRepository,
            IGenericRepository<ImportBank> importBankRepository,
            IGenericRepository<ImportMapping> importMappingRepository,
            IGenericRepository<TransactionCategory> transactionCategorieRepository,
            IGenericRepository<Transaction> transactionRepository)
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