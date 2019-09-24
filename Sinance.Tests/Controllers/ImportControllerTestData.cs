using System;
using System.Globalization;
using Finances.Domain.Entities;
using Finances.UnitTestBase.Classes;

namespace Finances.Web.Tests.Controllers
{
    /// <summary>
    /// Test data class for the import controller tests
    /// </summary>
    public class ImportControllerTestData : FinanceTestData
    {
        internal string IngImportFileContents;
        internal ImportBank IngImportBank;
        internal Transaction IngExpectedTransaction01;
        internal Transaction IngExpectedTransaction02;

        /// <summary>
        /// Upserts test data for an ING import
        /// </summary>
        public void Upsert_Ing_ImportConfiguration()
        {
            IngImportBank = UpsertImportBank();

            UpsertImportMapping(index: 0, bankAccountId: IngImportBank.Id, columnType: ColumnType.Name, columnName: "Naam", columnIndex: 1);
            UpsertImportMapping(index: 1, bankAccountId: IngImportBank.Id, columnType: ColumnType.AddSubtract, columnName: "Af/bij", columnIndex: 5, formatValue:"Af");
            UpsertImportMapping(index: 2, bankAccountId: IngImportBank.Id, columnType: ColumnType.Amount, columnName: "Bedrag", columnIndex: 6);
            UpsertImportMapping(index: 3, bankAccountId: IngImportBank.Id, columnType: ColumnType.Date, columnName: "Datum", columnIndex: 0, formatValue: "yyyyMMdd");
            UpsertImportMapping(index: 4, bankAccountId: IngImportBank.Id, columnType: ColumnType.Description, columnName: "Omschrijving", columnIndex: 8);
            UpsertImportMapping(index: 5, bankAccountId: IngImportBank.Id, columnType: ColumnType.DestinationAccount, columnName: "Tegenrekening", columnIndex: 3);
            UpsertImportMapping(index: 6, bankAccountId: IngImportBank.Id, columnType: ColumnType.BankAccountFrom, columnName: "rekening", columnIndex: 2);
        }

        /// <summary>
        /// Sets up the test data for an ing import
        /// </summary>
        public void Setup_Valid_Ing_Import()
        {
            IngExpectedTransaction01 = CreateTransaction(id: 0,
                bankAccountId: 0,
                applicationUserId: null,
                amount: (decimal)-18.69,
                date: new DateTime(2015, 03, 04),
                destinationAccount: "DESTINATIONACCOUNT1",
                name: string.Format(CultureInfo.CurrentCulture, "TestName_{0}", GenerateTestId(FinanceEntityType.Transaction, 0)),
                accountFrom: "IBANBANKACCOUNT1",
                isNegative: true,
                description: "TestDescription1");

            IngExpectedTransaction02 = CreateTransaction(id: 0,
                bankAccountId: 0,
                applicationUserId: null,
                amount: (decimal)-40.17,
                date: new DateTime(2015, 03, 05),
                destinationAccount: "DESTINATIONACCOUNT2",
                name: string.Format(CultureInfo.CurrentCulture, "TestName_{0}", GenerateTestId(FinanceEntityType.Transaction, 1)),
                accountFrom: "IBANBANKACCOUNT2",
                isNegative: true,
                description: "TestDescription2");

            IngImportFileContents = Resources.INGImport;
            IngImportFileContents = IngImportFileContents.Replace("[TRANSACTION_ONE_NAME]", IngExpectedTransaction01.Name);
            IngImportFileContents = IngImportFileContents.Replace("[TRANSACTION_TWO_NAME]", IngExpectedTransaction02.Name);
        }
    }
}
