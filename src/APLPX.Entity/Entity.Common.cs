using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    [DataContract]
    public class FilterGroup
    {
        [DataMember]
        public int FilterType { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<Filter> Filters { get; set; }
    }

    [DataContract]
    public class Filter
    {
        [DataMember]
        public int FilterType { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsSelectedInAnalytic { get; set; }

        [DataMember]
        public bool IsSelectedInPricing { get; set; }
    }

    [DataContract]
    public class    ValueDriver
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Sort { get; set; }

        [DataMember]
        public DriverType DriverType { get; set; }

        [DataMember]
        public bool IsInverted { get; set; }

        [DataMember]
        public string UnitOfMeasure { get; set; }

        [DataMember]
        public int Precision { get; set; }

        [DataMember]
        public bool IsSelected { get; set; }

        [DataMember]
        public decimal DriverMinimum { get; set; }

        [DataMember]
        public decimal DriverMaximum { get; set; }
    }

    [DataContract]
    public class ValueDriverGroup
    {
        [DataMember]
        public int GroupNumber { get; set; }
        [DataMember]
        public decimal? MinOutlier { get; set; }
        [DataMember]
        public decimal? MaxOutlier { get; set; }
        public decimal? MinActual { get; set; }
        public decimal? MaxActual { get; set; }


        public int SkuCount { get; set; }
        public decimal SalesValue { get; set; }
        public int Sort { get; set; }
    }

    public enum ValueDriverType
    {
        Analytic,
        PricingEveryday
    }


    [DataContract]
    public class PriceList
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string PriceListType { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public DateTime? EffectiveDate { get; set; }

        [DataMember]
        public DateTime? EndDate { get; set; }

        [DataMember]
        public int Sort { get; set; }

        [DataMember]
        public bool IsPromotion { get; set; }
    }

    [DataContract]
    public class PriceMarkupRule
    {
        [DataMember]
        public int Id { get; set; } //GroupNum?
        [DataMember]
        public decimal? DollarRangeLower { get; set; }
        [DataMember]
        public decimal? DollarRangeUpper { get; set; }
        [DataMember]
        public decimal PercentLimitLower { get; set; }
        [DataMember]
        public decimal? PercentLimitUpper { get; set; }
        [DataMember]
        public bool IsLowerRangeEditable { get; set; }
        [DataMember]
        public bool IsUpperRangeEditable { get; set; }

        
    }


    [DataContract]
    public class PriceOptimizationRule
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public decimal? DollarRangeLower { get; set; }
        [DataMember]
        public decimal? DollarRangeUpper { get; set; }
        [DataMember]
        public decimal PercentChange { get; set; }
        [DataMember]
        public bool IsLowerRangeEditable { get; set; }
        [DataMember]
        public bool IsUpperRangeEditable { get; set; }
    }

    [DataContract]
    public class PriceRoundingRule
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public RoundingType RoundingType { get; set; }
        [DataMember]
        public decimal? DollarRangeLower { get; set; }
        [DataMember]
        public decimal? DollarRangeUpper { get; set; }
        [DataMember]
        public decimal ValueChange { get; set; }
        [DataMember]
        public bool IsLowerRangeEditable { get; set; }
        [DataMember]
        public bool IsUpperRangeEditable { get; set; }
    }

    //public enum PriceListGroupType
    //{
    //    Dealer,
    //    Retail
    //}

    public enum DriverType
    {
        [Display(Name = "Movement")]
        Movement = 1,

        [Display(Name = "Days On Hand")]
        DaysOnHand = 2,

        [Display(Name = "Markup")]
        Markup = 3
    }

    public enum FilterType
    {
        DiscountType = 1,
        Hierarchy = 2,
        InventoryCatalogLine = 3,
        InventoryStatus = 4,
        Location = 5,
        PackageType = 6,
        PricingType = 7,
        ProductIntroduced = 8,
        ProductType = 9,
        StockSupplyClassification = 10,
        VendorCode = 11
    }

    public enum DriverGroupMode
    {
        Auto,
        Manual
    }

    public enum PricingMode
    {
        NotSet,
        [Display(Name = "Single", Description = "Only one price list is used.")]
        Single,
        [Display(Name = "Cascade", Description = "All non-Key Price Lists update based on a percentage of the Price List above.")]
        Cascade,
        [Display(Name = "Global Key", Description = "All non-Key Price Lists update based on a user set percentage applied to the Key Price List.")]
        GlobalKey,
        [Display(Name = "Global Key+", Description = "Individual SKUs in non-Key Price Lists retain their existing percentage ratio between themselves and the Key Price List.")]
        GlobalKeyPlus,
        [Display(Name = "Promotion", Description = "Target promotional SKUs use Key and Influencer Drivers to assign promotion pricing based on Key price list prices.")]
        Promotion

    }

    public enum PricingState
    {
        [Display(Name = "Not Set")]
        NotSet,

        [Display(Name = "In Progress")]
        InProgress,

        Submitted,
        Rejected,
        Approved,
        Implemented,
        Canceled,
        Failed,
        Reverting,
        Reverted
    }

    /// <summary>
    /// Specifies the types of updates available for a pricing approval request.
    /// </summary>
    public enum PricingUpdateType
    {
        [Display(Name = "Not Set")]
        NotSet = 0,

        [Display(Name = "Current Price")]
        Current = 1,

        [Display(Name = "Future Price")]
        Future = 2,

        [Display(Name = "Current and Future Prices")]
        CurrentAndFuture = 3
    }

    public enum TemplateType
    {
        NotSet = 0,
        Optimization = 1,
        Rounding = 2,
        Markup = 3
    }

    public enum RoundingType
    {
        [Display(Name = "Round Up")]
        RoundUp = 1,

        [Display(Name = "Round Down")]
        RoundDown = 2,

        [Display(Name = "Round Nearest")]
        RoundNear = 3,

        [Display(Name = "None")]
        None = 4
    }


    public enum LookupType
    {
        FilterType,
        Filters,
        DriverType,
        //DriverGroupMode,
        PriceLists,
        //PriceListType,
        UserRoleType,
        RoundingType,
        PricingMode,
        TemplateType
    }

    public interface IPricingTemplate
    {

        int Id { get; set; }
        TemplateType TemplateType { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        string Notes { get; set; }
        DateTime DateCreated { get; set; }
        //public List<Template> Rules { get; set; }

    }

    public class PriceMarkupTemplate : IPricingTemplate
    {
        public int Id { get; set; }
        public TemplateType TemplateType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }

        public List<PriceMarkupRule> Rules { get; set; }

    }


    public class PriceOptimizationTemplate : IPricingTemplate
    {
        public int Id { get; set; }
        public TemplateType TemplateType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }

        public List<PriceOptimizationRule> Rules { get; set; }
    }

    public class PriceRoundingTemplate : IPricingTemplate
    {
        public int Id { get; set; }
        public TemplateType TemplateType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }

        public List<PriceRoundingRule> Rules { get; set; }
    }

    // for use in template generation using percentile calculation
    public class PercentilePriceRange : PriceRange
    {
        public int SKUCount { get; set; }
    }

    public class PriceRange
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }

    public class ProductDetail
    {
        public string SkuId { get; set; }
        public int[] Filters { get; set; }
        public decimal Markup { get; set; }
        public decimal Movement { get; set; }
        public decimal DaysOnHand { get; set; }
        public decimal SalesValue { get; set; }
    }

    public class ResultSummary
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int SkuCount { get; set; }
        public int PriceCount { get; set; }
        public int WarningCount { get; set; }
        public int EditedCount { get; set; }
        public int ExcludedCount { get; set; }
    }

    public class GlobalParameter
    {

        public string ParamName { get; set; } // character varying(50) NOT NULL, -- Uniquely identifies the global paramater.
        public string ParamPrompt { get; set; }             // character varying(100) NOT NULL, -- This is the text displayed in the Administration module's list of parameters as a prompt for the user to specify a value.
        public string ParamDescription { get; set; } // character varying(250), -- If specified, it is used as the tool tip when the user hovers over the parameter prompt or the value fields in the UI.
        public string ParamValue { get; set; } // character varying(2000) NOT NULL, -- The current value for the parameter. This value could be of any data type, but it will be stored in a varchar and therefore in its string representation.
        public ParameterDomain ParamDomain { get; set; } // jsonb, -- CHANGE THIS DESCRIPTION AS JSON...
        public string ParamDefaultValue { get; set; }// character varying(2000) NOT NULL, -- This is the value that the ParamValue will be set to initially and whenever the user resets this paramater to its default.
    }

    public class ParameterDomain
    {
        public ParamDomainType ParamDomainType { get; set; }  // identifies the type of input field used to display/edit the parameter value

        public string FormatString { get; set; } // (optional) mask used for input

        public double MinValue { get; set; } // (optional) minimum value for numeric value
        public double MaxValue { get; set; } // (optional) maximum value for numeric value

        public bool MultilineText { get; set; }// (optional) indicates whether multi line input is allowed
        public int MaxLength { get; set; } // (optional) when set it limits the number of characters in the value of the parameter, otherwise limit is 2000
        public string[] ListValues { get; set; } // (needed when Type=List) list of values to use for dropdown
    }

    public enum ParamDomainType
    {
        Number,
        Text,
        List
    }

    public enum PriceRoutineType
    {
        NotSet = 0,
        Everyday,
        Promotion
    }

}
