using System;
using System.Runtime.Serialization;

namespace Sinance.Business.Exceptions
{
    [Serializable]
    public class ImportCacheEntryNotFoundException : Exception
    {
        public ImportCacheEntryNotFoundException()
        {
        }

        public ImportCacheEntryNotFoundException(string message) : base(message)
        {
        }

        protected ImportCacheEntryNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}