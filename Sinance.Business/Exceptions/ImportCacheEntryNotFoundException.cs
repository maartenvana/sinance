using System;

namespace Sinance.Business.Exceptions
{
    public class ImportCacheEntryNotFoundException : Exception
    {
        public ImportCacheEntryNotFoundException(string message) : base(message)
        {
        }
    }
}