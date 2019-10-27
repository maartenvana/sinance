using Serilog;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding.Seeds
{
    public class ImportBankSeed
    {
        private readonly ILogger _logger;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ImportBankSeed(
            ILogger logger,
            Func<IUnitOfWork> unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task SeedData()
        {
            using var unitOfWork = _unitOfWork();

            _logger.Information("Updating ImportBank definitions");

            await UpdateOrInsertINGBank(unitOfWork);

            await unitOfWork.SaveAsync();
        }

        private async Task<ImportBankEntity> InsertOrUpdateImportBank(IUnitOfWork unitOfWork, ImportBankEntity importBankEntity)
        {
            var existingImportBank = await unitOfWork.ImportBankRepository.FindSingleTracked(x => x.Name == importBankEntity.Name);
            if (existingImportBank == null)
            {
                unitOfWork.ImportBankRepository.Insert(importBankEntity);

                return importBankEntity;
            }
            else
            {
                existingImportBank.ImportContainsHeader = importBankEntity.ImportContainsHeader;
                existingImportBank.Delimiter = importBankEntity.Delimiter;

                return existingImportBank;
            }
        }

        private async Task InsertOrUpdateMappingsForBank(IUnitOfWork unitOfWork, ImportBankEntity importBank, List<ImportMappingEntity> importMappings)
        {
            var existingImportMappings = await unitOfWork.ImportMappingRepository.FindAllTracked(x => x.ImportBankId == importBank.Id);

            foreach (var existingImportMapping in existingImportMappings)
            {
                var importMapping = importMappings.SingleOrDefault(x => x.ColumnIndex == existingImportMapping.ColumnIndex);

                if (importMapping != null)
                {
                    existingImportMapping.ColumnName = importMapping.ColumnName;
                    existingImportMapping.ColumnTypeId = importMapping.ColumnTypeId;
                    existingImportMapping.FormatValue = importMapping.FormatValue;
                }
                else
                {
                    unitOfWork.ImportMappingRepository.Delete(existingImportMapping);
                }
            }

            var newImportMappings = importMappings.Where(x => !existingImportMappings.Any(y => y.ColumnIndex == x.ColumnIndex));
            foreach (var newImportMapping in newImportMappings)
            {
                newImportMapping.ImportBank = importBank;
                unitOfWork.ImportMappingRepository.Insert(newImportMapping);
            }
        }

        private async Task UpdateOrInsertINGBank(IUnitOfWork unitOfWork)
        {
            var importBank = await InsertOrUpdateImportBank(unitOfWork, new ImportBankEntity
            {
                Delimiter = ",",
                ImportContainsHeader = true,
                IsStandard = true,
                Name = "ING"
            });

            await InsertOrUpdateMappingsForBank(unitOfWork, importBank, new List<ImportMappingEntity>
                {
                    new ImportMappingEntity
                    {
                        ColumnIndex = 0,
                        ColumnName = "Datum",
                        ColumnTypeId = Communication.Model.Import.ColumnType.Date,
                        FormatValue = "yyyyMMdd"
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 1,
                        ColumnName = "Naam / Omschrijving",
                        ColumnTypeId = Communication.Model.Import.ColumnType.Name,
                        FormatValue = null
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 2,
                        ColumnName = "Rekening",
                        ColumnTypeId = Communication.Model.Import.ColumnType.BankAccountFrom,
                        FormatValue = null
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 3,
                        ColumnName = "Tegenrekening",
                        ColumnTypeId = Communication.Model.Import.ColumnType.DestinationAccount,
                        FormatValue = null
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 5,
                        ColumnName = "Af Bij",
                        ColumnTypeId = Communication.Model.Import.ColumnType.AddSubtract,
                        FormatValue = "Af"
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 6,
                        ColumnName = "Bedrag",
                        ColumnTypeId = Communication.Model.Import.ColumnType.Amount,
                        FormatValue = null
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 8,
                        ColumnName = "Mededelingen",
                        ColumnTypeId = Communication.Model.Import.ColumnType.Description,
                        FormatValue = null
                    }
                });
        }
    }
}