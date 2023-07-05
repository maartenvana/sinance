using System;
using System.Runtime.Serialization;

namespace Sinance.Business.Exceptions;

[Serializable]
public class DeleteStandardCategoryException : Exception
{
    public DeleteStandardCategoryException()
    {
    }

    public DeleteStandardCategoryException(string message) : base(message)
    {
    }

    protected DeleteStandardCategoryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}