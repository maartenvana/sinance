using Sinance.Communication.Model.Import;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sinance.Business.Import;

public interface IBankFileImporter
{
    Guid Id { get; }

    string FriendlyName { get; }

    IList<ImportRow> CreateImport(Stream fileStream);
}
