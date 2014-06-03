namespace Sieve.NET.Core.Exceptions
{
    using System;

    public class SievePropertyNotSetException : Exception
    {
        public SievePropertyNotSetException(string message)
            : base(message)
        {
            
        }
    }
}