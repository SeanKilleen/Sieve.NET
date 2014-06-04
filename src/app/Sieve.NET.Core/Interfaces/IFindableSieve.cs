using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieve.NET.Core.Interfaces
{
    public interface IFindableSieve<TTypeOfObjectToFilter, TPropertyType>
    {
        ISieve<TTypeOfObjectToFilter, TPropertyType> GetSieve(); 
    }
}
