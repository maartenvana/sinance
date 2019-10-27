using System;
using System.Runtime.Serialization;

namespace Sinance.Business.Exceptions
{
    [Serializable]
    public class ModelConversionException : Exception
    {
        public ModelConversionException()
        {
        }

        public ModelConversionException(string message) : base(message)
        {
        }

        protected ModelConversionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}