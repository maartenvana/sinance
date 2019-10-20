using System;

namespace Sinance.Business.Exceptions
{
    public class ImportFileException : Exception
    {
        public ImportFileException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}