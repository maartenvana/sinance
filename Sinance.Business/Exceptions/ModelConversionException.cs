using System;

namespace Sinance.Business.Exceptions
{
    public class ModelConversionException : Exception
    {
        public ModelConversionException(string message) : base(message)
        {
        }
    }
}