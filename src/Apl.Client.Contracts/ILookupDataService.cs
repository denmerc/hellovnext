using APLPX.Entity;
using System.Collections.Generic;

namespace APLPX.Client.Contracts
{
    /// <summary>
    /// Service contract for lookup services providers.
    /// </summary>
    public interface ILookupDataService
    {

        List<FilterGroup> LoadFilters();

        List<PriceList> LoadPriceLists( PriceRoutineType type );
    }

}
