using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    /// <summary>
    /// Top-level data transfer object for a Tracking Summary
    /// </summary>
    public class TrackingSummary
    {
        public TrackingSummary()
        {
            DataPointSets = new List<TrackingDataPointSeries>();
            ImplementedPriceRoutines = new List<ImplementedPricing>();
        }

        public List<TrackingDataPointSeries> DataPointSets { get; set; }
        public List<ImplementedPricing> ImplementedPriceRoutines { get; set; }
    }

    /// <summary>
    /// Data transfer object for a series of tracking data points having the same period granularity, e.g., week, month, period, etc.
    /// </summary>
    public class TrackingDataPointSeries
    {
        public TrackingDataPointSeries()
        {
            Points = new List<TrackingDataPoint>();
        }

        [DataMember]
        public CalculationInterval PeriodGranularityId { get; set; }

        [DataMember]
        public List<TrackingDataPoint> Points { get; set; }
    }

    /// <summary>
    /// Data transfer object for an individual tracking data point.
    /// </summary>
    public class TrackingDataPoint
    {
        [DataMember]
        public DateTime DataPointDate { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public decimal? UnitSales { get; set; }

        [DataMember]
        public decimal? Revenue { get; set; }

        [DataMember]
        public decimal? Cost { get; set; }

        [DataMember]
        public decimal? Profit { get; set; }

        [DataMember]
        public decimal? Markup { get; set; }

        public decimal? ImpactQty { get; set; }

        public decimal? ImpactAmount { get; set; }
    }  

    /// <summary>
    ///  Data transfer object for an implemented price routine.
    /// </summary>
    public class ImplementedPricing
    {
        [DataMember]
        public long TrackPricingId { get; set; }

        [DataMember]
        public DateTime ImplementationDate { get; set; }

        [DataMember]
        public string Name { get; set; }
        public string Description { get; set; }

        [DataMember]
        public string PricingType { get; set; }

        [DataMember]
        public PricingUpdateType UpdateTypeId { get; set; }

        [DataMember]
        public DateTime? PromotionStart { get; set; }

        [DataMember]
        public DateTime? PromotionEnd { get; set; }

        [DataMember]
        public int SKUCount { get; set; }

        [DataMember]
        public int LowerPriceCount { get; set; }

        [DataMember]
        public int HigherPriceCount { get; set; }

        [DataMember]
        public decimal? AvgPriceReduction { get; set; }

        [DataMember]
        public decimal? AvgPriceIncrease { get; set; }

        public string AnalyticName { get; set; }
        public DateTime AnalyticStartDate { get; set; }
        public DateTime AnalyticEndDate { get; set; }
    }

    public class TrackingPerformance
    {
        
        public List<TrackingDataPointSeries> Series { get; set; }

       
        public List<PerformanceResult> Results { get; set; }
    }

    public class PerformanceResult
    {
        public string Label { get; set; }
        public int UnitSales { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
    }

    public class TrackingCluster
    {
        public long ClusterId { get; set; }
        public List<DriverGroupCombination> EncodedSpace { get; set; }
        public int SkuCount { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitContributionPerSKU{ get; set; }
    }

    public class TrackingTimeFrame
    {
        public DateTime MinStartDate { get; set; }
        public DateTime MaxStartDate { get; set; }
    }

    public class TrackingProduct
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitContribution { get; set; }
    }

    public class TrackingPriceList
    {
        public int PriceListId { get; set; }
        public string PriceListName { get; set; }
        public int SkuCount { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitContribution { get; set; }
        public int Sort { get; set; }

    }
}
