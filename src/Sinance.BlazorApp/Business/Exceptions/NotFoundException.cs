using System;
using System.Runtime.Serialization;

namespace Sinance.BlazorApp.Business.Exceptions;

[Serializable]
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    private NotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
        throw new NotImplementedException();
    }
}
