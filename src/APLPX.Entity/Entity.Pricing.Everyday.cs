using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    [DataContract]
    public class PricingEveryday
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int AnalyticId { get; set; }
        public string AnalyticName { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Notes { get; set; }
        [DataMember]
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        [DataMember]
        public PricingMode PricingMode { get; set; }
        [DataMember]
        public int FolderId { get; set; }
        public List<FilterGroup> Filters { get; set; }
        public List<PricingPriceList> PriceLists { get; set; }
        [DataMember]
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        //not in db table
        [DataMember]
        public int? KeyValueDriverId { get; set; }
        [DataMember]
        public int? KeyPriceListId { get; set; }
        public List<PricingValueDriver> ValueDrivers { get; set; }

        public int? ApproverId { get; set; }
        public int PricingStateId { get; set; }
        public bool ChangeProductFiltersFlag { get; set; }

        public int SkuCount { get; set; }
        public int PriceCount { get; set; }
        public bool NeedsRecalculation { get; set; }
    }

    [DataContract]
    public class PricingValueDriver : ValueDriver
    {

        [DataMember]
        public bool IsKey { get; set; }
        public bool ChangeDriverFlag { get; set; }

        [DataMember]
        public List<PricingValueDriverGroup> Groups { get; set; }
    }


    public class PricingValueDriverGroup : ValueDriverGroup
    {
        //From Base ValueDriverGroup
        //GroupNumber
        //MinOutlier MaxOutlier Sort SkuCount SalesValue //TODO: these values need to be queried separately?
        public List<PriceOptimizationRule> OptimizationRules { get; set; }
        public decimal InfluencerPercentChange { get; set; }
        public bool ChangeGroupFlag { get; set; }
    }



    [DataContract]
    public class PricingPriceList : PriceList
    {
        public int PricingId { get; set; }
        public long TrackPricingId { get; set; }
        public int PriceListId { get; set; }
        public bool IsKey { get; set; } //table prop not in dto


        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal PercentChange { get; set; }
        public List<PriceRoundingRule> RoundingRules { get; set; }
        public bool ApplyRounding { get; set; }
        public List<PriceMarkupRule> MarkupRules { get; set; }
        public bool ApplyMarkup { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

    }

    [DataContract]
    public class PricingResult
    {
        [DataMember]
        public int SkuId { get; set; }
        [DataMember]
        public string SkuCode { get; set; }
        [DataMember]
        public string SkuDescription { get; set; } //name
        public decimal Cost { get; set; }
        public decimal Inventory { get; set; }
        //[DataMember]
        //public List<PricingResultDriverGroup> Groups { get; set; }
        [DataMember]
        public List<PricingResultDetail> PricingResultDetail { get; set; }
        public bool HasCompetitionData { get; set; }
        public int ClusterId { get; set; }
    }


    public class PricingResultDetail //pricing result in db
    {


        //[DataMember]
        //public int ResultId { get; set; }
        //public PriceList PriceList { get; set; }
        public int ProductId { get; set; }
        public int PriceListId { get; set; }
        public string PriceListName { get; set; }

        public decimal CurrentPrice { get; set; }

        public decimal? RecommendedPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public PricingResultsEditType PricingResultEditTypeId { get; set; }
        public List<Alert> Alerts { get; set; }

        public bool HasCompetitionData { get; set; }
        public decimal? CompetitionMinPrice { get; set; }
        public decimal? CompetitionMaxPrice { get; set; }
        public decimal? CompetitivePosition { get; set; }
        //[DataMember]
        //public int CurrentMarkupPercent { get; set; }
        //[DataMember]
        //public int NewMarkupPercent { get; set; }
        //[DataMember]
        //public decimal KeyValueChange { get; set; }
        //[DataMember]
        //public decimal InfluenceValueChange { get; set; }
        //[DataMember]
        //public decimal PriceChange { get; set; }
        //[DataMember]
        //public PricingResultWarningType PriceWarning { get; set; }
    }

    public class PricingResultValueDriverDetail
    {
        [DataMember]
        public int PriceListId { get; set; }
        [DataMember]
        public List<PricingValueDriverDetail> DriverDetail { get; set; }
    }

    public class PricingValueDriverDetail
    {
        [DataMember]
        public int DriverId { get; set; }
        [DataMember]
        public string DriverName { get; set; }
        [DataMember]
        public bool IsKey { get; set; }
        [DataMember]
        public decimal PriceChange { get; set; }
        public decimal DriverPercent { get; set; }
        public int DriverGroup { get; set; }

    }


    public class CompetitionData //pricing result in db
    {
        public int ProductId { get; set; }
        public string CompetitorName { get; set; }
        public string CompetitorSKU { get; set; }
        //public string SalesChannel { get; set; }
        public string CompetitorProductName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal CompetitorPrice { get; set; }

        public decimal? ShippingCost { get; set; }
        public bool? OutOfStockFlag { get; set; }

        public List<CompetitorAttribute> Attributes { get; set; }
    }

    public class CompetitorAttribute
    {
        public string Attribute { get; set; }
        public string Value { get; set; }

    }

    public class CompetitorSeries //pricing result in db
    {

        public string MetricName { get; set; }

        public string CompetitorName { get; set; }
        public string CompetitorSku { get; set; }
        //public string SalesChannel { get; set; }
        public string CompetitorProductName { get; set; }
        public bool IsCompetitor { get; set; }
        public int Order { get; set; }

        public List<CompetitorSeriesAttribute> DataPoints { get; set; }
    }

    public class CompetitorSeriesAttribute
    {
        public DateTime DataPointDate { get; set; }
        public decimal Value { get; set; }
    }

    public class Alert
    {
        public int AlertId { get; set; }
        public string AlertMessage { get; set; }
        public int Severity { get; set; }
    }

    public class PricingSummary
    {
        public long SkuCount { get; set; }
        public long PriceCount { get; set; }
    }

    public class PricingChartSummary
    {
        public int PriceListId { get; set; }
        public decimal MinPercentChange { get; set; }
        public decimal MaxPercentChange { get; set; }
        public decimal AvgPercentChange { get; set; }
        public decimal MinMarkupChange { get; set; }
        public decimal MaxMarkupChange { get; set; }
        public decimal AvgMarkupChange { get; set; }
    }

}
