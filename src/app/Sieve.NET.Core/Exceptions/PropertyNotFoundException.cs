namespace Sieve.NET.Core.Exceptions
{
    using System;

    public class PropertyNotFoundException : ApplicationException
    {
        public PropertyNotFoundException(string message)
            : base(message)
        {
        }
    }
}