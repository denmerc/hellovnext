using System;
using System.Collections.Generic;
using APLPX.Entity;

namespace APLPX.Client.Contracts
{
    public interface IPricingDataService
    {

        List<PricingEveryday> LoadList(int ownerId, PriceRoutineType priceRoutineType);

        PricingEveryday Load(int pricingId, bool includeChildren = false);

        long Save(Entity.PricingEveryday priceRoutine);

        PricingEveryday Create(int folderId, int userId, PriceRoutineType priceRoutineType); //NewAnalytic-123

        PricingEveryday Copy(int pricingId); // name + copy

        List<FilterGroup> LoadFilters(int pricingId);

        bool SaveFilters(int pricingId, FilterGroup[] filterIds);

        List<PricingPriceList> LoadPriceLists(int pricingId);

        bool SavePriceLists(int pricingId, PricingMode mode, List<PricingPriceList> priceListsToSave);

        List<PricingValueDriver> LoadDrivers(int pricingId);

        bool SaveDrivers(int pricingId, List<PricingValueDriver> valueDriversToSave);

        List<PricingResult> LoadResults(int pricingId, PricingResultFilter[] filters, IProgress<String> progress);

        PricingResult LoadResults(int pricingId, int productId);

        PricingResultValueDriverDetail GetResultDetail(int pricingId, int productId);

        List<PricingResult> SaveResults(int pricingId, PricingResultFilter[] filters, List<PricingResult> resultsToSave);

        List<PricingResult> RecalculateResults(int pricingId, PricingResultFilter[] filters, IProgress<String> progress);

        ResultSummary GetResultSummary(int pricingId);

        long GetResultCount(int pricingId);

        AnalyticAggregates GetAnalyticAggregates(int pricingId);
        bool SaveFolderAssignment(int entityId, int folderId);

        ImpactAnalysis CreateImpactAnalysis(int pricingId);

        ImpactAnalysis LoadImpactAnalysis(int impactAnalysisId);

        bool DeleteImpactAnalysis(int impactAnalysisId);

        List<ImpactAnalysis> LoadImpactAnalyses(int pricingId);

        long SaveImpactAnalysis(ImpactAnalysis analysis);

        bool SaveImpactAnalyses(IEnumerable<ImpactAnalysis> impactAnalyses);

        ImpactAnalysis RunImpactAnalysis(ImpactAnalysis analysis, IProgress<String> progress);

        List<CompetitionData> LoadCompetitionData(int productId);

        List<CompetitorSeries> LoadCompetitionSeries(int productId, int priceListId);

        int GetFilterSkuCount(IEnumerable<FilterGroup> filters);

        int GetPriceListSkuCount(int pricingId, IEnumerable<int> priceListIds);

        List<PricingChartSummary> GetResultsChartSummary(int pricingId);

        void DeleteCompetitionProduct(int productId, string competitorName, string competitorSku);


    }


}
