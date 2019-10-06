using System;

namespace Sinance.Business.Exceptions.Authentication
{
    public class IncorrectPasswordException : Exception
    {
        public IncorrectPasswordException(string message) : base(message)
        {
        }
    }
}