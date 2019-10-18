using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Business.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string itemName) : base($"{itemName} not found")
        {
        }
    }
}