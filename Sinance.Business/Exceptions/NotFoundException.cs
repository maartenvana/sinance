using System;

namespace Sinance.Business.Exceptions
{
    public class NotFoundException : Exception
    {
        public string ItemName { get; }

        public NotFoundException(string itemName) : base($"{itemName} not found")
        {
            ItemName = itemName;
        }
    }
}