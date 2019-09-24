using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Business.Exceptions.Authentication
{
    public class IncorrectPasswordException : Exception
    {
        public IncorrectPasswordException(string message) : base(message)
        {
        }
    }
}