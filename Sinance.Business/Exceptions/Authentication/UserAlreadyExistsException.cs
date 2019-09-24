using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Business.Exceptions.Authentication
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string message) : base(message)
        {
        }
    }
}