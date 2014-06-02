namespace Sieve.NET.Core.Options
{
    /// <summary>
    /// Indicates a behavior that should take place if the values list is empty.
    /// </summary>
    /// <remarks>The default, 0, is to let all objects through.</remarks>
    public enum EmptyValuesListBehavior
    {
        LetAllObjectsThrough = 0, //this is the default because it is 0.
        LetNoObjectsThrough,
        ThrowSieveValuesNotFoundException
    }
}