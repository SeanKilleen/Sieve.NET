namespace Sieve.NET.Core.Options
{
    /// <summary>
    /// A behavior to use when the Sieve encounters an invalid acceptable value.
    /// </summary>
    /// <remarks>The default, 0, is to ignore the invalid value.</remarks>
    public enum InvalidValueBehavior
    {
        IgnoreInvalidValue = 0, //default
        ThrowInvalidSieveValueException
    }
}