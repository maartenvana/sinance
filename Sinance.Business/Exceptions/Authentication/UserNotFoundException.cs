using System;

namespace Sinance.Business.Exceptions.Authentication
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}