using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;


namespace APLPX.Client.Postgres.Models
{
    [Schema("PX_Main")]
    public class Pricing
    {
        [AutoIncrement]
        public int PricingId { get; set; }
        public int AnalyticId { get; set; }
        [Ignore]
        public string AnalyticName { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        [StringLength(250)]
        [Required]
        public string Description { get; set; }
        [StringLength(2000)]
        public string Notes { get; set; }
        [References(typeof(User))]
        public int OwnerId { get; set; }
        public int PricingModeId { get; set; }
        public int FolderId { get; set; }
        [CustomField("jsonb")]
        public ProductFilterGroup[] ProductFilters { get; set; }
        [Reference]
        public List<PricingDriver> Drivers { get; set; }
        [Reference]
        public List<PricingPriceList> PriceLists { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime ModifyTS { get; set; }

        public int? ApproverId { get; set; }
        public int PricingStateId { get; set; }
        public bool ChangeProductFiltersFlag { get; set; }
        public bool NeedsRecalculation { get; set; }



    }

    //public class PricingFilter
    //{
    //    [References(typeof(Pricing))]
    //    [Required]
    //    public int PricingId { get; set; }
    //    [References(typeof(Filter))]
    //    [Required]
    //    public int FilterId { get; set; }
    //    //[StringLength(50)]
    //    //public string FilterName { get; set; }
    //    //[References(typeof(FilterType))]
    //    // public int FilterType { get; set; }
    //}

    [Schema("PX_Main")]
    [Alias("PricingDriverRules")]
    public class PricingDriver
    {
        [References(typeof(Pricing))]
        public int PricingId { get; set; }
        //[References(typeof(DriverType))]
        public int DriverId { get; set; } //DriverId
        public bool? IsKey { get; set; }
        [CustomField("jsonb")]
        [Alias("DriverOptimization")]
        public PriceOptimizationRule[] OptimizationRules { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime ModifyTS { get; set; }
        public bool ChangeDriverFlag { get; set; }


    }

    public class PricingDriverExtended : PricingDriver
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        [CustomField("jsonb")]
        public DriverGroup[] Encoding { get; set; }
    }

    //public class PricingDriverGroup : DriverGroup //TODO: No table???
    //{        
    //    public List<PriceOptimizationRule> OptimizationRules { get; set; }
    //}


    public class PriceMarkupRule
    {
        //public int GroupNum { get; set; }
        public decimal? DollarRangeLower { get; set; }
        public decimal? DollarRangeUpper { get; set; }
        public decimal PercentLimitLower { get; set; }
        public decimal? PercentLimitUpper { get; set; }
    }


    public class PriceOptimizationRule
    {
        public int GroupNum { get; set; }
        public PriceOptimizationRange[] PriceRanges { get; set; }
        public decimal InfluencerPercentChange { get; set; }
        public bool ChangeGroupFlag { get; set; }
        public decimal? MinOutlier { get; set; }
        public decimal? MaxOutlier { get; set; }
    }


    public class PriceOptimizationRange
    {
        public decimal? DollarRangeLower { get; set; }
        public decimal? DollarRangeUpper { get; set; }
        public decimal PercentChange { get; set; }
    }


    public class PriceRoundingRule : TemplateRule
    {
        public int RoundingType { get; set; }
        public decimal ValueChange { get; set; }
    }



    [Schema("PX_Main")]
    public class PricingPriceList
    {
        [References(typeof(Pricing))]
        [Required]
        public int PricingId { get; set; }

        [References(typeof(PriceList))]
        [Required]
        public int PriceListId { get; set; }

        public bool IsKey { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal PercentChange { get; set; }

        [CustomField("jsonb")]
        [Alias("RoundingRule")]
        public List<PriceRoundingRule> RoundingRules { get; set; }

        [CustomField("jsonb")]
        [Alias("MarkupRule")]
        public List<PriceMarkupRule> MarkupRules { get; set; }

        public bool ApplyRounding { get; set; }

        public bool ApplyMarkup { get; set; }

        public DateTime CreateTS { get; set; }
        public DateTime ModifyTS { get; set; }

    }

    [Schema("PX_Main")]
    public class PricingPriceListExtended : PricingPriceList
    {      
        //Fields from the base PriceList table:
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


    [Schema("PX_Main")]
    public class PricingMode
    {
        [PrimaryKey]
        public int PricingModeId { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
    }

    [Schema("PX_Main")]
    public class PricingState
    {
        [PrimaryKey]
        public int PricingStateId { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
    }


    [Schema("PX_Main")]
    public class PricingRecommendation
    {
        [References(typeof(Pricing))]
        public int PricingId { get; set; }
        [References(typeof(Product))]
        public int ProductId { get; set; }
        [References(typeof(PriceList))]
        public int PriceListId { get; set; }
        public decimal FinalPrice { get; set; } //edit field edited by user
        public decimal RecommendedPrice { get; set; }
        public int PricingResultEditTypeId { get; set; } //edit
        public List<Alert> Alerts { get; set; }
        //public decimal CurrentPrice { get; set; }//readonly
        //public int CurrentMarkupPercent { get; set; }//readonly
        //public int NewMarkupPercent { get; set; } //TODO: calculated fields
        //public decimal KeyValueChange { get; set; } 
        //public decimal InfluenceValueChange { get; set; }
        //public decimal PriceChange { get; set; } //end computed fields
        //[References(typeof(PricingResultEditType))] //<--ui validation 
        //[References(typeof(PricingResultWarningType))]
        //public int PricingResultWarningType { get; set; }//-->
    }

    public class PricingResult //response object when calling SP_PricingLoadResults
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public decimal Cost { get; set; }
        public decimal Inventory { get; set; }
        public int ClusterId { get; set; }
        [CustomField("json")]
        public PricingResultDetail[] PricingResultDetail { get; set; }
        public bool HasCompetitionData { get; set; }
    }

    public class PricingResultDetail
    {
        public int ProductId { get; set; }
        public int PriceListId { get; set; }
        public string PriceListName { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal RecommendedPrice { get; set; }
        public int PricingResultEditType { get; set; }
        public Alert[] Alerts { get; set; }
        public decimal FinalPrice { get; set; }
        public bool HasCompetitionData { get; set; }
        public decimal? CompetitionMinPrice { get; set; }
        public decimal? CompetitionMaxPrice { get; set; }
        public decimal? CompetitivePosition { get; set; }
    }

    public class PricingResultValueDriverDetail//response object when calling SP_PricingLoadResults
    {
        public int PriceListId { get; set; }
        [CustomField("json")]
        public PricingValueDriverDetail[] PriceDrivers { get; set; }
    }

    public class PricingValueDriverDetail//response object when calling SP_PricingLoadResults
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public bool IsKey { get; set; }
        public decimal PriceChange { get; set; }
        public decimal DriverPercent { get; set; }
        public int DriverGroup { get; set; }
    }

    public class Alert
    {
        public int AlertId { get; set; }
        public string AlertMessage { get; set; }
        public int Severity { get; set; }
    }

    [Schema("PX_Main")]
    public class PricingResultEditType
    {

        [PrimaryKey]
        public int PricingResultEditTypeId { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

    }
    [Schema("PX_Main")]
    public class PricingResultWarningType
    {

        [PrimaryKey]
        public int Id { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

    }

    [Schema("PX_Main")]
    public class ImpactScenario
    {
        [AutoIncrement]
        [PrimaryKey]
        public int ImpactId { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }

        public int PricingId { get; set; }
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

    [Serializable]
    public class ImpactAnalysisCalculation
    {
        [CustomField("jsonb")]
        public ImpactAnalysisDataPointSet[] Forecast { get; set; }

        [CustomField("jsonb")]
        public ImpactAnalysisResultSet[] Results { get; set; }
    }

    public class ImpactAnalysisResultSet
    {
        public int PeriodGranularityId { get; set; }
        public ImpactAnalysisResult[] Items { get; set; }
    }

    public class ImpactAnalysisDataPointSet
    {
        public int PeriodGranularityId { get; set; }
        public DataPoint[] Points { get; set; }
    }

    public class ImpactAnalysisResult
    {
        public string Name { get; set; }
        public int UnitOfMeasureId { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal ProjectedAmount { get; set; }
        public decimal ImpactAmount { get; set; }
    }

    public class DataPoint
    {
        public DateTime DataPointDate { get; set; }
        public string Label { get; set; }

        public decimal? ActualQty { get; set; }
        public decimal? TrendQty { get; set; }
        public decimal? ProjectedQty { get; set; }
        public decimal? ImpactQty { get; set; }

        public decimal? ActualAmount { get; set; }
        public decimal? TrendAmount { get; set; }
        public decimal? ProjectedAmount { get; set; }
        public decimal? ImpactAmount { get; set; }
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

    //class PricingResult2
    //{
    //    //public PricingEveryday PriceRoutine { get; set; }
    //    public Product Product { get; set; }
    //    public List<ProductPriceChange> PriceChanges { get; set; }
    //}

    //public class ProductPriceChange
    //{
    //    public PriceList PriceList { get; set; }
    //    public decimal CurrentPrice { get; set; }
    //    public decimal FinalPrice { get; set; }
    //    public decimal RecommendedPrice { get; set; }
    //    public int PricingResultEditType { get; set; }
    //}

    //public class Product
    //{
    //    [DataMember]
    //    public int Id { get; set; }
    //    [DataMember]
    //    public string Name { get; set; }
    //    [DataMember]
    //    public string Description { get; set; }

    //    public decimal Cost { get; set; }


    //}



    //public class Driver
    //{
    //    public int Id { get; set; }
    //    [References(typeof(Analytic))]
    //    public int EntityId { get; set; }
    //    [References(typeof(DriverEntityType))]
    //    public int DriverEntityType { get; set; } 

    //    public bool IsAuto { get; set; }
    //    public List<DriverGroup> Groups { get; set; }

    //}

    //public enum DriverEntityType
    //{
    //    Analytic,
    //    Pricing
    //}




}
