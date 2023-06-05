﻿using Sinance.Storage.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Storage;

public interface IUnitOfWork 
{
    IGenericRepository<BankAccountEntity> BankAccountRepository { get; }

    IGenericRepository<CategoryMappingEntity> CategoryMappingRepository { get; }

    IGenericRepository<CategoryEntity> CategoryRepository { get; }

    SinanceContext Context { get; }

    IGenericRepository<CustomReportCategoryEntity> CustomReportCategoryRepository { get; }

    IGenericRepository<CustomReportEntity> CustomReportRepository { get; }

    IGenericRepository<TransactionCategoryEntity> TransactionCategoryRepository { get; }

    IGenericRepository<TransactionEntity> TransactionRepository { get; }

    IGenericRepository<SinanceUserEntity> UserRepository { get; }

    Task<int> SaveAsync();
}