using System;
using System.Runtime.Serialization;

namespace Sinance.Business.Exceptions.Authentication;

[Serializable]
public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException()
    {
    }

    public UserAlreadyExistsException(string message) : base(message)
    {
    }

    protected UserAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}