using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;

namespace APLPX.Client.Postgres.Models
{
    [Schema("PX_Main")]
    public class Analytic
    {
        [AutoIncrement]
        [PrimaryKey]
        public int AnalyticId { get; set; }
        [StringLength(50)]
        [Required]
        [Index(Unique=true)]
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
        [References(typeof(User))]
        public int OwnerId { get; set; }
        public DateTime CreateTS { get; set; }
        public DateTime ModifyTS { get; set; }
        public DateTime AggStartDate { get; set; }
        public DateTime AggEndDate { get; set; }

    }

    //public class AnalyticFilter
    //{
    //    [AutoIncrement]
    //    public int Id { get; set; }
    //    [References(typeof(Filter))]
    //    [Required]
    //    public int FilterId { get; set; }
    //    [References(typeof(Analytic))]
    //    [Required]
    //    public int AnalyticId { get; set; }
    //    //[StringLength(50)]
    //    //public string FilterName { get; set; }
    //    //[References(typeof(FilterType))]
    //    // public int FilterType { get; set; }
    //}

    [Schema("PX_Main")]    
    public class AnalyticDriver
    {
        [References(typeof(Analytic))]
        public int AnalyticId { get; set; }
        public int DriverId { get; set; }
        [CustomField("jsonb")]
        public DriverEncoding Encoding { get; set; } //Groups
        //[CustomField("jsonb")]
        //public EncodingResult[] EncodingResult { get; set; } //Groups
        public DateTime CreateTS { get; set; }
        public DateTime ModifyTS { get; set; }


        //public Boolean IsAuto { get; set; }
        //public List<DriverGroup> Groups { get; set; }

    }

    public class AnalyticDriverExtended : AnalyticDriver
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsInverted { get; set; }
        public string UnitOfMeasure { get; set; }

    }


    //public class AnalyticPriceList
    //{
    //    [AutoIncrement]
    //    public int Id { get; set; }
    //    //TODO: add clustered primary key
    //    //public int Id { get; set; } 
    //    [References(typeof(Analytic))]
    //    [Required]
    //    public int AnalyticId { get; set; }
    //    [References(typeof(PriceList))]
    //    [Required]
    //    public int PriceListId { get; set; }
    //}

    [Schema("PX_Main")]
    public class AnalyticSummary
    {
        public long SkuCount { get; set; }
        public decimal TotalSalesValue { get; set; }
    }

    [Schema("PX_Main")]
    public class DriverCombinationAggregate
    {
        public int ClusterId { get; set; }
        [CustomField("json")]
        public DriverGroupCombination[] Cluster { get; set; }
        public long SkuCount { get; set; }
        public decimal SalesValue { get; set; }
    }

    public class AnalyticProduct
    {
        public int AnalyticId { get; set; }
        public int ProductId { get; set; }
        public decimal PlannedSalesAmount { get; set; }
        public decimal ActualSalesAmount { get; set; }
        public decimal SalesQuantity { get; set; }
    }
}
