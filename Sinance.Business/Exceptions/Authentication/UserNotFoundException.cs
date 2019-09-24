using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Business.Exceptions.Authentication
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}