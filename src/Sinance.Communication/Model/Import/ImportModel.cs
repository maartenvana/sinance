using Sinance.Communication.Model.BankAccount;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Communication.Model.Import;

public class ImportModel
{
    public IList<BankAccountModel> AvailableAccounts { get; set; }

    public IList<ImportBankModel> AvailableImportBanks { get; set; }

    [Display(Name = "Bankrekening")]
    [Required]
    public int BankAccountId { get; set; }

    [Display(Name = "Import Type")]
    [Required]
    public Guid BankFileImporterId { get; set; }

    public IList<ImportRow> ImportRows { get; set; }
}