using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieve.NET.Core.Exceptions
{
    /// <summary>
    /// Indicates that a value supplied to a Sieve as an acceptable value is actually invalid.
    /// </summary>
    public class InvalidSieveValueException : Exception
    {
        public InvalidSieveValueException(string message)
            : base(message)
        {
        }
    }
}
