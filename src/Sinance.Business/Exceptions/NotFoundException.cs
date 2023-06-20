using System;
using System.Runtime.Serialization;

namespace Sinance.Business.Exceptions;

[Serializable]
public class NotFoundException : Exception
{
    public string ItemName { get; }

    public NotFoundException(string itemName) : base($"{itemName} not found")
    {
        ItemName = itemName;
    }

    public NotFoundException()
    {
    }

    protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}