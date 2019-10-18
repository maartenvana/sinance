using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Business.Exceptions
{
    public class ModelConversionException : Exception
    {
        public ModelConversionException(string message) : base(message)
        {
        }
    }
}