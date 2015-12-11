using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    //[DataContract]
    //public class PricingIdentity
    //{

    //    [DataMember]
    //    public int AnalyticId { get; set; }
    //    [DataMember]
    //    public string Name { get; set; }
    //    [DataMember]
    //    public string Description { get; set; }
    //    [DataMember]
    //    public string Notes { get; set; }
    //    public DateTime Created { get;  set; }
    //    [DataMember]
    //    public string Owner { get;  set; }
    //    [DataMember]
    //    public bool Shared { get; set; }
    //    [DataMember]
    //    public bool Active { get;  set; }
    //}

    //[DataContract]
    //public class PricingMode
    //{


    //    [DataMember]
    //    public int Key { get;  set; }
    //    [DataMember]
    //    public string Name { get;  set; }
    //    [DataMember]
    //    public string Title { get;  set; }
    //    [DataMember]
    //    public bool HasKeyPriceListRule { get;  set; }
    //    [DataMember]
    //    public bool HasLinkedPriceListRule { get;  set; }
    //    [DataMember]
    //    public int KeyPriceListGroupKey { get;  set; }
    //    [DataMember]
    //    public int LinkedPriceListGroupKey { get;  set; }
    //    [DataMember]
    //    public short Sort { get;  set; }
    //}

    //[DataContract]
    //public class PricingKeyPriceListRule
    //{


    //    [DataMember]
    //    public int PriceListId { get; set; }
    //    [DataMember]
    //    public decimal DollarRangeLower { get; set; }
    //    [DataMember]
    //    public decimal DollarRangeUpper { get; set; }
    //    [DataMember]
    //    public List<PriceRoundingRule> RoundingRules { get; set; }

    //}

    //[DataContract]
    //public class PricingLinkedPriceListRule
    //{


    //    [DataMember]
    //    public int PriceListId { get; set; }
    //    [DataMember]
    //    public int PercentChange { get; set; }     
    //    [DataMember]
    //    public List<PriceRoundingRule> RoundingRules { get; set; }
    //}

    //[DataContract]
    //public class PricingMarkupTemplate
    //{


    //    //[DataMember]
    //    //public int Id { get;  set; }
    //    [DataMember]
    //    public string Name { get;  set; }
    //    [DataMember]
    //    public string Description { get;  set; }
    //    [DataMember]
    //    public short Sort { get;  set; }
    //    [DataMember]
    //    public List<PriceMarkupRule> Rules{ get;  set; }
    //}

    //[DataContract]
    //public class PricingOptimizationTemplate
    //{


    //    [DataMember]
    //    public int Id { get;  set; }
    //    [DataMember]
    //    public string Name { get;  set; }
    //    [DataMember]
    //    public string Description { get;  set; }
    //    [DataMember]
    //    public short Sort { get;  set; }
    //    [DataMember]
    //    public List<PriceOptimizationRule> Rules { get;  set; }
    //}

    //[DataContract]
    //public class PricingRoundingTemplate
    //{
    //    [DataMember]
    //    public int Id { get;  set; }
    //    [DataMember]
    //    public string Name { get;  set; }
    //    [DataMember]
    //    public string Description { get;  set; }
    //    [DataMember]
    //    public short Sort { get;  set; }
    //    [DataMember]
    //    public List<PriceRoundingRule> Rules { get;  set; }
    //}

    //[DataContract]
    //public class PricingValueDriverGroup : ValueDriverGroup
    //{
    //    [DataMember]
    //    public int SkuCount { get;  set; }
    //    [DataMember]
    //    public string SalesValue { get;  set; }
    //}

    
    public class PricingResultFilter
    {
        
        public ResultFilterKey Key { get; set; }
        public ResultFilterType Type { get; set; }
        public string[] Values { get; set; }
        //[DataMember]
        //public string Name { get; set; }
        //[DataMember]
        //public string Title { get; set; }
        //[DataMember]
        //public short Sort { get; set; }
    }

    public enum ResultFilterType
    {
        InList,
        InclusiveRange
    }

    public enum ResultFilterKey
    {
        CurrentPrice,
        FinalPrice,
        PriceListId,
        WarningType,
        EditType
    }

    [DataContract]
    public class PricingResultEdit
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public PricingResultsEditType Type { get; set; }
    }

    [DataContract]
    public class PricingResultWarning
    {
        [DataMember]
        public string Name { get;  set; }
        [DataMember]
        public string Title { get;  set; }
        [DataMember]
        public PricingResultsWarningType Type { get;  set; }
    }

    [DataContract]
    public class PricingResultDriverGroup : ValueDriverGroup
    {
        [DataMember]
        public string Name { get;  set; }
        [DataMember]
        public string Title { get;  set; }
        [DataMember]
        public string Actual { get;  set; }      
    }

    public class PricingApprovalRequest
    {
        public int UserId { get; set; }
        public int PriceRoutineId { get; set; }
        public int ApproverId { get; set; }
        public string Comments { get; set; }
        public DateTime TransmitTS { get; set; }
        public int[] ImpactAnalyses { get; set; }
        public PricingUpdateType ApprovalUpdateType { get; set; }
    }

    public enum ApprovalActionType
    {
        NotSet = 0,
        Submit,
        Reject,
        Approve,
        Cancel,
        SystemTransmit,
        SystemFail,
        Retransmit,
        RevertPrices,
        RevertTransmit
    }

    public class ImpactAnalysis
    {
        public int ImpactId { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }

        public int PricingId { get; set; }        
        public decimal ElasticityRegulator { get; set; }
        public decimal DefaultPriceElasticity { get; set; }
        public int TrendCalcLength { get; set; }
        public DateTime AnalyticStartDate { get; set; }
        public DateTime AnalyticEndDate { get; set; }
        public DateTime ProjectedStartDate { get; set; }
        public DateTime ProjectedEndDate { get; set; }
        public ImpactAnalysisResultSet[] Results { get; set; }
        public ImpactAnalysisDataPointSet[] Forecast { get; set; }

    }

    public class ImpactAnalysisResultSet
    {
        public CalculationInterval PeriodGranularityId { get; set; }
        public ImpactAnalysisResult[] Items { get; set; }
    }

    public class ImpactAnalysisDataPointSet
    {
        public CalculationInterval PeriodGranularityId { get; set; }
        public DataPoint[] Points { get; set; }
    }

    public class ImpactAnalysisResult
    {
        public string Name { get; set; }
        public UnitOfMeasureType UnitOfMeasureType { get; set; }
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
}
