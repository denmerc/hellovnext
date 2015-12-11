using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    [DataContract]
    public class Analytic
    {

        public Analytic() {}

        [DataMember]
        public int Id { get;  set; }
        [DataMember]
        public int FolderId { get;  set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Notes { get; set; }
        [DataMember]
        public DateTime CreatedDate { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
        public int OwnerId { get; set; }
        [DataMember]
        public string OwnerName { get; set; }
        //public int OwnerId { get; set; }
        //[DataMember]
        //public bool IsShared { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public List<AnalyticValueDriver> ValueDrivers { get; set; }
        [DataMember]
        public FilterGroup[] Filters { get;  set; }
        public int[] PriceLists { get; set; }
        public DateTime AggregationStartDate { get; set; }
        public DateTime AggregationEndDate { get; set; }

        public int SkuCount { get; set; }
        public decimal TotalSales { get; set; }
        public int DriverCount { get; set; }
    }

    //[DataContract]
    //public class AnalyticIdentity
    //{

    //    [DataMember]
    //    public string Name { get; set; }
    //    [DataMember]
    //    public string Description { get; set; }
    //    [DataMember]
    //    public string Notes { get; set; }
    //    [DataMember]
    //    public DateTime Created { get;  set; }
    //    [DataMember]
    //    public string Owner { get;  set; }
    //    [DataMember]
    //    public bool Shared { get; set; }
    //    [DataMember]
    //    public bool Active { get; set; }
    //}

    [DataContract]
    public class AnalyticValueDriver : ValueDriver 
    {

        //public bool IsAuto { get; set; }       
        public List<ValueDriverGroup> Groups { get; set; }
        public decimal MinDriverOutlier { get; set; }
        public decimal MaxDriverOutlier { get; set; }
    }

    public class EncodingResult
    {
        public string Type { get; set; }
        public List<ValueDriverGroup> Result { get; set; }
    }

    public class AnalyticAggregates
    {
        public long SkuCount { get; set; }
        public decimal TotalSalesValue { get; set; }

    }
    public class MinMax
    {

        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }

    public class DriverOutliers
    {
        public int SkusBelowLowerLimit { get; set; }
        public int SkusAboveUpperLimit { get; set; }
        public decimal SalesValueBelowLowerLimit { get; set; }
        public decimal SalesValuAboveUpperLimit { get; set; }
    }
    public class DriverCombinationAggregate
    {
        public int ClusterId { get; set; }
        public DriverGroupCombination[] Cluster { get; set; }
        public long SkuCount { get; set; }
        public decimal TotalSalesValue { get; set; }
    }

    public class DriverGroupCombination
    {
        public int DriverId { get; set; }
        public int GroupNum { get; set; }
    }
    //[DataContract]
    //public class AnalyticValueDriverMode : ValueDriverMode
    //{
    //    [DataMember]
    //    public List<ValueDriverGroup> Groups { get;  set; }
    //}

    //[DataContract]
    //public class AnalyticResultValueDriverGroup : ValueDriverGroup
    //{

    //    [DataMember]
    //    public int SkuCount { get;  set; }
    //    [DataMember]
    //    public string SalesValue { get;  set; }
    //}

    //[DataContract]
    //public class AnalyticPriceListGroup : PriceListGroup
    //{

    //    [DataMember]
    //    public List<PriceList> PriceLists { get;  set; }
    //}
}




