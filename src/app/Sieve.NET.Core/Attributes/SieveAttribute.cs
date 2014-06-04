using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieve.NET.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SieveAttribute : Attribute
    {
        public SieveAttribute(string filterName, string PropertyNameFilteringOn = "")
        {
            this.FilterName = filterName;
            this.PropertyToCheck = PropertyNameFilteringOn;
            
            if (string.IsNullOrWhiteSpace(PropertyToCheck))
            {
                PropertyToCheck = FilterName;
            }
        }

        public string FilterName { get; private set; }
        public string PropertyToCheck { get; private set; }
    }
}
