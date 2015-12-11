using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;


namespace APLPX.Client.Postgres.Models
{
    [Schema("PX_Main")]
    public class GlobalParameter
    {
        [PrimaryKey]
        [StringLength(50)]
        [Required]
        public string ParamName { get ;set;} // character varying(50) NOT NULL, -- Uniquely identifies the global paramater.
        [StringLength(100)]
        [Required]
        public string ParamPrompt { get; set; }             // character varying(100) NOT NULL, -- This is the text displayed in the Administration module's list of parameters as a prompt for the user to specify a value.
        [StringLength(250)]
        public string ParamDescription { get; set; } // character varying(250), -- If specified, it is used as the tool tip when the user hovers over the parameter prompt or the value fields in the UI.
        [StringLength(2000)]
        [Required]
        public string ParamValue { get; set; } // character varying(2000) NOT NULL, -- The current value for the parameter. This value could be of any data type, but it will be stored in a varchar and therefore in its string representation.
        [Required]
        [CustomField("jsonb")]
        public ParameterDomain ParamDomain { get; set; } // jsonb, -- CHANGE THIS DESCRIPTION AS JSON...
        [StringLength(2000)]
        [Required]
        public string ParamDefaultValue { get; set; }// character varying(2000) NOT NULL, -- This is the value that the ParamValue will be set to initially and whenever the user resets this paramater to its default.
        DateTime ModifyTS { get; set; } // timestamp without time zone NOT NULL DEFAULT now()
    }


    public class ParameterDomain
    {
        public int ParamDomainType { get; set; }  // identifies the type of input field used to display/edit the parameter value

        public string FormatString { get; set; } // (optional) mask used for input

        public double MinValue { get; set; } // (optional) minimum value for numeric value
        public double MaxValue { get; set; } // (optional) maximum value for numeric value

        public bool MultilineText { get; set; }// (optional) indicates whether multi line input is allowed
        public int MaxLength { get; set; } // (optional) when set it limits the number of characters in the value of the parameter, otherwise limit is 2000
        public string[] ListValues { get; set; } // (needed when Type=List) list of values to use for dropdown
    }
    
    [Schema("PX_Main")]
    public class FilterValue
    {
        [PrimaryKey] // compound primary key FilterTypeId, Value
        [Required]
        [References(typeof(FilterType))]
        public int FilterTypeId { get; set; }
        [PrimaryKey] 
        [StringLength(50)]
        [Required]
        public string Value { get; set; }
        [StringLength(50)]
        public string ValueDescription { get;  set; }

        public long BatchID { get; set; }
        //public int Key { get; set; }

    }

    public class ProductFilterGroup //maps to save to ProductFilters column in Analytic & Pricing
    {       
        public int FilterTypeId { get; set; }
        public string Name { get; set; }
        public string[] Values { get; set; }
    }

    public class PricingProductFilterGroup
    {
        public int FilterTypeId { get; set; }
        public string Name { get; set; }
        public PricingFilterValue[] Values { get; set; }
    }

    public class PricingFilterValue
    {
        public string Value { get; set; }
        public bool AnalyticSelected { get; set; }
        public bool PricingSelected { get; set; }
    }

    [Schema("PX_Main")]
    [Alias("FilterType")]
    public class FilterGroup
    {
        public int FilterTypeId { get; set; }
        [Alias("KeyName")]
        public string Name { get; set; }
        [Reference]
        public List<FilterValue> Filters { get; set; }

    }

    /// <summary>
    /// DB entity for a Price List.
    /// </summary>
    [Schema("PX_Main")]
    public class PriceList
    {        
        [AutoIncrement]
        public int PriceListId { get; set; }

        public string PriceListType { get; set; }
     
        [StringLength(25)]
        [Required]
        public string Code { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public DateTime? EffectiveDate { get; set; }
      
        public DateTime? EndDate { get; set; }

        public short Sort { get; set; }

        public long BatchID { get; set; }
    }

    public class DriverEncoding
    {
        public decimal MinDriverOutlier { get; set; }
        public decimal MaxDriverOutlier { get; set; }
        public DriverGroup[] Groups { get; set; }
    }

    [Schema("PX_Main")]
    public class DriverGroup //encoding db field - encoding result added
    {
        //public int Id { get; set; }
        //encoding
        public int GroupNumber { get; set; }
        public decimal? MinOutlier { get; set; }
        public decimal? MaxOutlier { get; set; }

        // encoding result
        public decimal? MinActual { get; set; }
        public decimal? MaxActual { get; set; }
        //used to return from AutoCalculate and RecalculateAggregatess
        public int SkuCount { get; set; }
        public decimal SalesValue { get; set; }
    }

    [Schema("PX_Main")]
    public class DriverGroupCombination
    {
        public int DriverId { get; set; }
        public decimal DriverValue { get; set; }
    }

    //[Schema("PX_Main")]
    //public class Encoding 
    //{
    //    [CustomField("jsonb")]
    //    public string Type { get; set; }
    //    [CustomField("jsonb")]
    //    public EncodingDefinition[] Definition { get; set; }
    //}

    //[Schema("PX_Main")]
    //public class EncodingDefinition
    //{
    //    public int GroupNum { get; set; }
    //    public decimal RangeMin { get; set; }
    //    public decimal RangeMax { get; set; }

    //}

    //[Schema("PX_Main")]
    //public class EncodingResult
    //{
    //    [CustomField("jsonb")]
    //    public string Type { get; set; }
    //    [CustomField("jsonb")]
    //    public EncodingResultDefinition[] Result { get; set; }
    //}
    
    //[Schema("PX_Main")]
    //public class EncodingResultDefinition
    //{
    //    public int GroupNum { get; set; }
    //    public decimal SalesAmount { get; set; }
    //    public decimal SalesQuantity { get; set; }
    //    public decimal ProductCount { get; set; }
    //}

    [Schema("PX_Main")]
    public class DriverType
    {
        [PrimaryKey]
        public int DriverId { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsInverted { get; set; }
        public string UnitOfMeasure { get; set; }
    }

    [Schema("PX_Main")]    
    public class FilterType
    {
        [PrimaryKey]
        public int FilterTypeId { get; set; }
        [StringLength(50)]
        [Required]
        public string KeyName { get; set; }
        public long BatchID { get; set; }
    }




    [Schema("PX_Main")]
    public class Role
    {
        [PrimaryKey]
        public int RoleId { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
    }

    [Schema("PX_Main")]
    public class User
    {
        [PrimaryKey]
        [AutoIncrement]
        public int UserId { get; set; }
        [StringLength(50)]
        [Required]
        [Index(Unique=true)]
        public string Login { get; set; }
        [StringLength(100)]
        [Required]
        public string Password { get; set; }
        [StringLength(50)]
        [Required]
        public string FirstName { get; set; }
        [StringLength(50)]
        [Required]
        public string LastName { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        [CustomField("jsonb")]
        public int[] Roles { get; set; }
        [Required]
        [CustomField("jsonb")]
        public Folder[] FolderList { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime ModifyTS { get; set; }
        public string Email { get; set; }
        //public DateTime DateCreated { get; set; }
        //[StringLength(1000)] //Cap of 6 folders //default set of 6 tags
        //public string[] SearchGroups { get; set; }
    }


    public class Folder
    {
        public int ModuleFeatureId{ get; set; }
        public int FolderId { get; set; }
        public string Name { get; set; }
    }


    [Schema("PX_Main")]
    public class Module
    {
        [PrimaryKey]
        public int Id { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        [StringLength(100)]
        [Required]
        public string Description { get; set; }
        [Reference]
        public List<ModuleFeature> ModuleFeatures { get; set; }
        public int[] Roles { get; set; }

    }

    [Schema("PX_Main")]
    public class ModuleFeature
    {
        [PrimaryKey]
        public int Id { get; set; }
        [References(typeof(Module))]
        [Required]
        public int ModuleId { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        public int[] Roles { get; set; }
        //public string[] SearchGroups { get; set; } //TODO: should this be a lookup
        
    }

    public class SearchGroup
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string SearchGroupName { get; set; }
        [References(typeof(User))]
        public int UserId { get; set; }
    }

    
    //public class UserSearchGroup
    //{
    //    [AutoIncrement]
    //    public int Id { get; set; }
    //    public int SearchGroupId { get; set; }
    //    [References(typeof(User))]
    //    public int UserId { get; set; }
    //}


    public enum LookupType
    {

        Filters,
        PriceLists
        //FilterType,
        //DriverType,
        //DriverGroupMode,        //PriceListType,
        //UserRoleType,
        //RoundingType,
        //PricingMode,
        //TemplateType
    }

    public enum ParamDomainType
    {
        Number,
        Text,
        List
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


    public class MinMax
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
    }

    #region Templates



    public class RoundingTemplate : Template
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(TemplateType))]
        [Required]
        public int TemplateType { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        [StringLength(100)]
        [Required]
        public string Description { get; set; }
        [StringLength(2000)]
        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }
        public List<RoundingTemplateRule> Rules { get; set; }

    }


    public class OptimizationTemplate : Template
    {
        [PrimaryKey]
        public int Id { get; set; }
        [References(typeof(TemplateType))]
        [Required]
        public int TemplateType { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        [StringLength(100)]
        [Required]
        public string Description { get; set; }
        [StringLength(2000)]
        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }
        public List<OptimizationTemplateRule> Rules { get; set; }

    }


    public class MarkupTemplate : Template
    {
        [PrimaryKey]
        public int Id { get; set; }
        [References(typeof(TemplateType))]
        [Required]
        public int TemplateType { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        [StringLength(100)]
        [Required]
        public string Description { get; set; }
        [StringLength(2000)]
        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }
        public List<MarkupTemplateRule> Rules { get; set; }

    }

    [Schema("PX_Main")]
    public class Template
    {
        [PrimaryKey]
        [AutoIncrement]
        [Required]
        public int TemplateId { get; set; }
        [References(typeof(TemplateType))]
        [Required]
        public int TemplateType { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
        [StringLength(250)]
        public string Description { get; set; }
        [StringLength(2000)]
        public string Notes { get; set; }
        [CustomField("jsonb")]
        public string Rules { get; set; }
        public DateTime CreateTS { get; set; }
        //public DateTime DateCreated { get; set; }

    }

    
    public class TemplateType
    {
        [PrimaryKey]
        public int Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
    }
    
    [Schema("PX_Main")]
    public class RoundingType
    {
        [PrimaryKey]
        public int Id { get; set; }
        [StringLength(50)]
        [Required]
        public string Name { get; set; }
    }

    public class TemplateRule
    {
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }

        //public int RoundingType { get; set; }
        //public decimal OptimizationPercentage { get; set; }
        //public decimal MinMarkup { get; set; }
        //public decimal MaxMarkup { get; set; }

    }

    public class RoundingTemplateRule : TemplateRule
    {
        [References(typeof(RoundingType))]
        public int RoundingType { get; set; }
        public decimal RoundingValue { get; set; }
    }

    public class OptimizationTemplateRule : TemplateRule
    {
        public decimal PercentChange { get; set; }        
    }

    public class MarkupTemplateRule : TemplateRule
    {
        public decimal PercentLimitLower { get; set; }
        public decimal PercentLimitUpper { get; set; }
    }

    // for use in template generation using percentile calculation
    public class PercentilePriceRange: PriceRange
    {
        public int SKUCount { get; set; }
    }

    public class PriceRange
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }

    #endregion
}
