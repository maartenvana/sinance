using System;
using System.Runtime.Serialization;

namespace Sinance.Business.Exceptions.Authentication;

[Serializable]
public class UserNotFoundException : Exception
{
    public UserNotFoundException()
    {
    }

    public UserNotFoundException(string message) : base(message)
    {
    }

    protected UserNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}