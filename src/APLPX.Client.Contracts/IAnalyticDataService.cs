using System;
using System.Collections.Generic;
using APLPX.Entity;

namespace APLPX.Client.Contracts
{
    public interface IAnalyticDataService : IRepository<Analytic>
    {
        List<Analytic> LoadList(int ownerId);

        Analytic Load(int analyticId, bool includeChildren = false);

        long Save(Analytic analytic);

        Analytic Create(int folderId, int userId);

        Analytic Copy(int analyticId);

        FilterGroup[] LoadFilters(int analyticId);

        bool SaveFilters(int analyticId, FilterGroup[] filtersToSave);

        int[] LoadPriceLists(int analyticId);

        bool SavePriceLists(int analyticId, int[] priceListsToSave);

        List<AnalyticValueDriver> LoadDrivers(int analyticId);

        bool SaveDrivers(int analyticId, List<AnalyticValueDriver> valueDriversToSave);

        int CacheAnalyticProductSet(int analyticId, IProgress<String> progress);

        bool BuildClusters(int analyticId);

        ValueDriverGroup[] RecalculateAggregates(int analyticId, int driverId, ValueDriverGroup[] driverGroups);


        //TODO: dm bool invertGrouping 
        ValueDriverGroup[] AutoCalculate(int analyticId, int driverId, int groupCount, decimal minOutlier, decimal maxOutlier);
        ValueDriverGroup[] AutoCalculate(int analyticId, int driverId, int groupCount);

        AnalyticAggregates GetSummary(int analyticId);

        MinMax GetDriverSummary(int analyticId, int driverId);

        List<DriverCombinationAggregate> GetDriverCombinationAggregates(int analyticId);

        bool SaveFolderAssignment(int analyticId, int folderId);

        int GetFilterSkuCount(IEnumerable<FilterGroup> filters);

        int GetPriceListSkuCount(int analyticId, IEnumerable<int> priceListIds);
    }
}