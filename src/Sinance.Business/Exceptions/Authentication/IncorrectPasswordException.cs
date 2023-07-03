using System;
using System.Runtime.Serialization;

namespace Sinance.Business.Exceptions.Authentication
{
    [Serializable]
    public class IncorrectPasswordException : Exception
    {
        public IncorrectPasswordException()
        {
        }

        public IncorrectPasswordException(string message) : base(message)
        {
        }

        protected IncorrectPasswordException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}