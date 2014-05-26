namespace Sieve.NET.Core.Tests
{
    using System;

    public class PropertyNotFoundException : ApplicationException
    {
        public PropertyNotFoundException()
        {
        }
        public PropertyNotFoundException(string message)
            : base(message)
        {
        }
    }
}