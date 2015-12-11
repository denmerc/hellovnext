using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace APLPX.Client.Postgres.Models
{
    [Schema("PX_Track")]
    public class TrackPricing
    {
        [AutoIncrement]
        [PrimaryKey]
        public long TrackPricingId { get; set; }
        public int PricingId { get; set; }
        public int TrackAnalyticId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public int OwnerId { get; set; }
        public int ApproverId { get; set; }

        public int PricingModeId { get; set; }
        public int FolderId { get; set; }

        [CustomField("jsonb")]
        public ProductFilterGroup[] ProductFilters { get; set; }

        public DateTime TransmitTS { get; set; }

        public int PricingStateId { get; set; }
    }

    [Schema("PX_Track")]
    public class TrackPricingExtended : TrackPricing
    {
        public string OwnerName { get; set; }
        public string ApproverName { get; set; }
        public string PriceRoutineType { get; set; }
        public List<PricingApprovalEvent> ApprovalEvents { get; set; }
    }

    [Schema("PX_Track")]
    public class PricingApprovalEvent
    {
        [AutoIncrement]
        public long EventId { get; set; }
        public long TrackPricingId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ActionId { get; set; }
        public DateTime EventTS { get; set; }
        public string Comments { get; set; }
    }

    public class ExportPrice
    {
        public string PriceListCode { get; set; }
        public string SKU { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPriceCurrent { get; set; }
        public decimal? NewPriceFuture { get; set; }
    }

    [Schema("PX_Track")]
    [Alias("TrackPricingDriverRules")]
    public class TrackPricingDriver
    {
        [References(typeof(TrackPricing))]
        public int TrackPricingId { get; set; }
        //[References(typeof(DriverType))]
        public int DriverId { get; set; } //DriverId
        public bool IsKey { get; set; }

        [CustomField("jsonb")]
        [Alias("DriverOptimization")]
        public PriceOptimizationRule[] OptimizationRules { get; set; }
        public bool ChangeDriverFlag { get; set; }

    }

    [Schema("PX_Track")]
    [Alias("TrackPricingDriverRules")]
    public class TrackPricingDriverExtended : TrackPricingDriver
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        [CustomField("jsonb")]
        public DriverGroup[] Encoding { get; set; }

    }


    [Schema("PX_Track")]
    public class TrackPricingPriceList
    {
        [References(typeof(TrackPricing))]
        [Required]
        public int TrackPricingId { get; set; }
        [References(typeof(PriceList))]
        [Required]
        public int PriceListId { get; set; }

        public bool IsKey { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal PercentChange { get; set; }
        [CustomField("jsonb")]
        [Alias("RoundingRule")]
        public List<PriceRoundingRule> RoundingRules { get; set; }
        public bool ApplyRounding { get; set; }
        [CustomField("jsonb")]
        [Alias("MarkupRule")]
        public List<PriceMarkupRule> MarkupRules { get; set; }
        public bool ApplyMarkup { get; set; }
    }

    public class TrackPricingPriceListExtended : TrackPricingPriceList
    {
        public string PriceListType { get; set; }

        [StringLength(25)]
        [Required]
        public string PriceListCode { get; set; }

        [StringLength(50)]
        [Required]
        public string PriceListName { get; set; }

        [StringLength(100)]
        public string PriceListDescription { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Sort { get; set; }
    }


    [Schema("PX_Track")]
    public class TrackPricingRecommendation
    {
        [References(typeof(TrackPricing))]
        public int TrackPricingId { get; set; }
        [References(typeof(Product))]
        public int ProductId { get; set; }
        [References(typeof(PriceList))]
        public int PriceListId { get; set; }
        public decimal FinalPrice { get; set; } //edit field edited by user
        public decimal RecommendedPrice { get; set; }
        public List<Alert> Alerts { get; set; }
    }

    [Schema("PX_Track")]
    public class TrackingSummary
    {
        [CustomField("jsonb")]
        public TrackingDataPointCollection[] Series { get; set; }

        [CustomField("jsonb")]
        public ImplementedPricing[] ImplementedPricing { get; set; }
    }

    [Schema("PX_Track")]
    public class TrackingPerformance
    {
        [CustomField("jsonb")]
        public TrackingDataPointCollection[] Series { get; set; }

        [CustomField("jsonb")]
        public PerformanceResult[] Results { get; set; }
    }

    public class PerformanceResult
    {
        public string Label { get; set; }
        public int UnitSales { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
    }

    [Schema("PX_Track")]
    public class TrackingDataPointCollection
    {
        public int PeriodGranularityId { get; set; }
        public TrackingDataPoint[] Points { get; set; }
    }


    [Schema("PX_Track")]
    public class TrackingDataPoint
    {
        public DateTime DataPointDate { get; set; }
        public string Label { get; set; }
        public decimal? UnitSales { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Profit { get; set; }
        public decimal? Markup { get; set; }
        public decimal? ImpactQty { get; set; }
        public decimal? ImpactAmount { get; set; }
    }

    [Schema("PX_Track")]
    public class ImplementedPricing
    {
        [References(typeof(TrackPricing))]
        public long TrackPricingId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PricingType { get; set; }
        public int UpdateTypeId { get; set; }
        public DateTime ImplementationDate { get; set; }
        public DateTime? PromotionStart { get; set; }
        public DateTime? PromotionEnd { get; set; }
        public int SKUCount { get; set; }
        public int LowerPriceCount { get; set; }
        public int HigherPriceCount { get; set; }
        public decimal? AvgPriceReduction { get; set; }
        public decimal? AvgPriceIncrease { get; set; }
        public string AnalyticName { get; set; }
        public DateTime AnalyticStartDate { get; set; }
        public DateTime AnalyticEndDate { get; set; }
    }

    public class TrackingCluster
    {
        public long ClusterId { get; set; }
        public DriverGroupCombination[] EncodedSpace { get; set; }
        public int SkuCount { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitContributionPerSKU { get; set; }

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

    [Schema("PX_Track")]
    public class TrackImpactScenario
    {
        [AutoIncrement]
        [PrimaryKey]
        public long TrackImpactId { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }

        public long TrackPricingId { get; set; }
        public decimal ElasticityRegulator { get; set; }
        public decimal DefaultPriceElasticity { get; set; }
        public int TrendCalcDays { get; set; }
        public DateTime ProjectedStartDate { get; set; }
        public DateTime ProjectedEndDate { get; set; }

        [CustomField("jsonb")]
        public ImpactAnalysisDataPointSet[] Forecast { get; set; }

        [CustomField("jsonb")]
        public ImpactAnalysisResultSet[] Results { get; set; }
    }

    [Schema("PX_Track")]
    public class TrackAnalytic
    {
        [AutoIncrement]
        [PrimaryKey]
        public long TrackAnalyticId { get; set; }
        [StringLength(50)]
        [Required]
        [Index(Unique = true)]
        public string Name { get; set; }
        [StringLength(250)]
        [Required]
        public string Description { get; set; }
        [StringLength(2000)]
        public string Notes { get; set; }
        public int FolderID { get; set; }
        [CustomField("jsonb")]
        public int[] PriceLists { get; set; }
        [CustomField("jsonb")]
        public ProductFilterGroup[] ProductFilters { get; set; }
        //public int AggregationDays { get; set; }
        public string ClusterType { get; set; }
        public DateTime AggStartDate { get; set; }
        public DateTime AggEndDate { get; set; }
    }

}
