using System;
using System.Runtime.Serialization;

namespace Sinance.Business.Exceptions
{
    [Serializable]
    public class ImportFileException : Exception
    {
        public ImportFileException()
        {
        }

        public ImportFileException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ImportFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}