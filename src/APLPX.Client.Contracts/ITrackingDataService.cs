using System;
using System.Collections.Generic;
using APLPX.Entity;


namespace APLPX.Client.Contracts
{
    /// <summary>
    /// Interface for Tracking data service providers.
    /// </summary>
    public interface ITrackingDataService
    {
        TrackingSummary GetTrackingSummary(DateTime startDate, DateTime endDate);

        TrackingPerformance GetTrackingPerformance(long trackingPricingId,
                                                   DateTime baseStartDate, DateTime baseEndDate,
                                                   DateTime trackStartDate, DateTime trackEndDate,
                                                   IEnumerable<int> clusters, IEnumerable<int> priceLists, IEnumerable<int> products, 
                                                   int? impactAnalysisId = null);

        List<TrackingCluster> GetPerformanceClusters(long trackingPricingId, DateTime trackStartDate, DateTime trackEndDate);

        List<TrackingProduct> GetPerformanceProducts(long trackingPricingId, IEnumerable<int> trackPriceListIds, DateTime trackStartDate, DateTime trackEndDate);

        List<TrackingPriceList> GetPerformancePriceLists(long trackingPricingId, DateTime trackStartDate, DateTime trackEndDate);

        TrackingTimeFrame GetComparisonSyncTimeFrame(IEnumerable<int> trackPricingIds);

        ImpactAnalysis LoadImpactAnalysis(long impactAnalysisId);

        List<ImpactAnalysis> LoadImpactAnalyses(long trackPricingId);

        TrackedPriceRoutine LoadPricing(int pricingId);

        Analytic LoadAnalytic(int analyticId);

        AnalyticAggregates GetAnalyticAggregates(int analyticId);

        List<PricingResult> LoadResults(int pricingId, PricingResultFilter[] filters, IProgress<String> progress);

        PricingResultValueDriverDetail GetResultDetail(int pricingId, int productId);
    }
}
