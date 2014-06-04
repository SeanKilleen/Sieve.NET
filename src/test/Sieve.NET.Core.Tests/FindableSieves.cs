namespace Sieve.NET.Core.Tests
{
    using Sieve.NET.Core.Attributes;
    using Sieve.NET.Core.Interfaces;
    using Sieve.NET.Core.Sieves;

    [Sieve("AnInt")]
    public class AFindableSieve : IFindableSieve<ABusinessObject, int>
    {
        public ISieve<ABusinessObject, int> GetSieve()
        {
            return new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt);
        }

        public string CustomName = "";
        public string PropertyName = "AnInt";

    }

    [Sieve("ADateTimeStart", "ADateTime")]
    public class DateTimeStartFilter : IFindableSieve<ABusinessObject, string>
    {

        public ISieve<ABusinessObject, string> GetSieve()
        {
            return new EqualitySieve<ABusinessObject>().ForProperty(x => x.AString);
        }
    }

    [Sieve("ADateTimeEnd", "ADateTime")]
    public class DateTimeEndFilter : IFindableSieve<ABusinessObject, string>
    {

        public ISieve<ABusinessObject, string> GetSieve()
        {
            return new EqualitySieve<ABusinessObject>().ForProperty(x => x.AString);
        }
    }

}
