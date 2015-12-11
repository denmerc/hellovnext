using ServiceStack.DataAnnotations;
using System;


namespace APLPX.Client.Postgres.Models
{


    public class ProductDetail
    {
        [PrimaryKey]
        public string SkuId { get; set; }
        public int[] Filters { get; set; }
        public decimal Markup { get; set; }
        public decimal Movement { get; set; }
        public decimal DaysOnHand { get; set; }
        public decimal SalesValue { get; set; }
    }


    public class Product
    {
        [PrimaryKey]
        public int Id { get; set; } //sku
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public decimal InStockRatio { get; set; }
        public decimal DaysLeadTime { get; set; }
        public int ProductTypeId { get; set; } //filters
        public int StatusId { get; set; } 
        public int VendorId { get; set; }
        public int StockSupplyClassificationId { get; set; }
        public DateTime IntroductionDateId { get; set; }
        public int CatalogLineId { get; set; }
        public int DiscountTypeId { get; set; }
        public int HierarchyId { get; set; }
    }

    public class ProductPrice
    {
        [References(typeof(Product))]
        public int SkuId { get; set; }
        [References(typeof(PriceList))]
        public int PriceListId { get; set; }
        public decimal Price { get; set; }
        //public decimal FuturePrice { get; set; }
    }



}
