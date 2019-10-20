using System;

namespace Sinance.Business.Exceptions
{
    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException(string itemName) : base($"{itemName} already exists")
        {
        }
    }
}