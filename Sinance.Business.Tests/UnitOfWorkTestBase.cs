using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq.AutoMock;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Tests
{
    public class UnitOfWorkTestBase
    {
        protected readonly AutoMocker _mocker;
        private readonly DbContextOptions<SinanceContext> _dbContextOptions;

        public UnitOfWorkTestBase()
        {
            _mocker = new AutoMocker(Moq.MockBehavior.Strict);

            _dbContextOptions = new DbContextOptionsBuilder<SinanceContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

            SetupUnitOfWork();
        }

        protected async Task<bool> EntityExistsById<T>(int id) where T : EntityBase
        {
            using var context = new SinanceContext(_dbContextOptions);
            return await context.Set<T>().AnyAsync(x => x.Id == id);
        }

        protected void InsertEntities<T>(IEnumerable<T> bankAccountEntities) where T : EntityBase
        {
            using var context = new SinanceContext(_dbContextOptions);
            context.Set<T>().AddRange(bankAccountEntities);
            context.SaveChanges();
        }

        protected void InsertEntities<T>(params T[] bankAccountEntities) where T : EntityBase
        {
            using var context = new SinanceContext(_dbContextOptions);
            context.Set<T>().AddRange(bankAccountEntities);
            context.SaveChanges();
        }

        protected void InsertEntity<T>(T bankAccountEntity) where T : EntityBase
        {
            using var context = new SinanceContext(_dbContextOptions);

            context.Set<T>().Add(bankAccountEntity);
            context.SaveChanges();
        }

        private void SetupUnitOfWork()
        {
            _mocker.Use<Func<IUnitOfWork>>(() =>
            {
                var context = new SinanceContext(_dbContextOptions);

                return new UnitOfWork(
                    context,
                    new GenericRepository<SinanceUserEntity>(context),
                    new GenericRepository<BankAccountEntity>(context),
                    new GenericRepository<CategoryEntity>(context),
                    new GenericRepository<CategoryMappingEntity>(context),
                    new GenericRepository<CustomReportCategoryEntity>(context),
                    new GenericRepository<CustomReportEntity>(context),
                    new GenericRepository<ImportBankEntity>(context),
                    new GenericRepository<ImportMappingEntity>(context),
                    new GenericRepository<TransactionCategoryEntity>(context),
                    new GenericRepository<TransactionEntity>(context)
                );
            });
        }
    }
}