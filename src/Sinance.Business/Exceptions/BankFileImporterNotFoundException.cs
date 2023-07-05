using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sinance.Business.Exceptions;

[Serializable]
public sealed class BankFileImporterNotFoundException : Exception
{
    public BankFileImporterNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private BankFileImporterNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
