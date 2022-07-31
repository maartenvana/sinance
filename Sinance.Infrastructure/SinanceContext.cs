using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sinance.Domain.Model;
using Sinance.Infrastructure.EntityConfigurations;
using Sinance.Infrastructure.Extensions;
using Sinance.Infrastructure.Model;
using System.Data;

namespace Sinance.Infrastructure
{
    public class SinanceContext : DbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;
        private readonly IUserIdProvider _userIdProvider;

        private IDbContextTransaction _currentTransaction;

        public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

        public bool HasActiveTransaction => _currentTransaction != null;

        public DbSet<Account> BankAccounts { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<CategoryMapping> CategoryMappings { get; set; }

        public DbSet<AccountTransaction> Transactions { get; set; }

        public DbSet<ImportTransaction> SourceTransactions { get; set; }

        public DbSet<SinanceUser> Users { get; set; }

        /// <summary>
        /// Default constructors
        /// </summary>
        public SinanceContext(
            DbContextOptions<SinanceContext> options,
            IMediator mediator,
            IUserIdProvider userIdProvider)
            : base(options)
        {
            _mediator = mediator;
            _userIdProvider = userIdProvider;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null) return null;

            _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            ValidateTransactionCommit(transaction);

            try
            {
                await SaveChangesAsync();
                transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        private void ValidateTransactionCommit(IDbContextTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch Domain Events collection. 
            // Choices:
            // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
            // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
            // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
            // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
            await _mediator.DispatchDomainEventsAsync(this);

            // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
            // performed through the DbContext will be committed
            await base.SaveChangesAsync(cancellationToken);

            return true;
        }


        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountEntityTypeConfiguration(_userIdProvider));
            modelBuilder.ApplyConfiguration(new ImportTransactionEntityTypeConfiguration(_userIdProvider));
            modelBuilder.ApplyConfiguration(new AccountTransactionEntityTypeConfiguration(_userIdProvider));
            modelBuilder.ApplyConfiguration(new CategoryMappingEntityTypeConfiguration(_userIdProvider));
            modelBuilder.ApplyConfiguration(new CategoryEntityTypeConfiguration(_userIdProvider));
            modelBuilder.ApplyConfiguration(new SinanceUserEntityTypeConfiguration());
        }
    }
}