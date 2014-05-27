namespace Sieve.NET.Core.Options
{
    public enum EmptyValuesListBehavior
    {
        LetAllObjectsThrough = 0, //this is the default because it is 0.
        LetNoObjectsThrough,
        ThrowSieveValuesNotFoundException
    }
}