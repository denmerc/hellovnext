using ServiceStack.DataAnnotations;
using System;

namespace APLPX.Client.Postgres.Models
{
    public class CompetitionView
    {
        [PrimaryKey]
        public int ProductId { get; set; }
        public string CompetitorName { get; set; }
        public string CompetitorSku { get; set; }
        //public string SalesChannel { get; set; }
        public string CompetitorProductName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal CompetitorPrice { get; set; }

        public decimal? ShippingCost { get; set; }
        public bool? OutOfStockFlag { get; set; }

        [CustomField("json")]
        public CompetitorAttribute[] Attributes { get; set; }



    }

    public class CompetitorAttribute
    {
        public string Attribute { get; set; }
        public string Value { get; set; }

    }

    public class CompetitorSeries
    {
        public string MetricName { get; set; }

        public string CompetitorName { get; set; }
        public string CompetitorSku { get; set; }
        //public string SalesChannel { get; set; }
        public string CompetitorProductName { get; set; }
        public bool IsCompetitor { get; set; }
        public int Order { get; set; }

        [CustomField("json")]
        public CompetitorSeriesAttribute[] DataPoints { get; set; }
    }

    public class CompetitorSeriesAttribute
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }


    [Schema("PX_Competition")]
    public class CompetitionImportError
    {
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string ErrorMessage { get; set; }
    }


    [Schema("PX_Competition")]
    public class result
    {

        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }

        [Required]
        public string filename { get; set; }

        public string format { get; set; }

        public int rowsinfile { get; set; }

        public int rowsimported { get; set; }

        public int rowswitherror { get; set; }

        public string errorfilepath { get; set; }

        public string errorfilename { get; set; }

        public DateTime starttime { get; set; }

        public DateTime endtime { get; set; }

        public string duration { get; set; }

        public string sourcefilepath { get; set; }

        public int UserId { get; set; }       


    }
}
