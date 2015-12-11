using System;
using System.Runtime.Serialization;


namespace APLPX.Entity
{
    [DataContract]
    public class ImportData
    {
        /// <summary>
        /// Line number from import file
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Property for sku's
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// Property for product company name
        /// </summary>
        public string Company { get; set; }


        /// <summary>
        /// Property for sku's
        /// </summary>
        public string CompetitorSKU { get; set; }

        ///// <summary>
        ///// Property for FileName
        ///// </summary>
        //[DataMember]
        //public string FileName { get; set; }

        ///// <summary>
        ///// Property for File Format
        ///// </summary>
        //[DataMember]
        //public string Format { get; set; }

        /// <summary>
        /// Property for Product name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Property for Sales Channel in which the price for the competing product is found
        /// </summary>
        public string SalesChannel { get; set; }

        /// <summary>
        /// Property for product price 
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Property for shipping
        /// </summary>
        public double Shipping { get; set; }

        /// <summary>
        /// Property for in stock
        /// </summary>
        public string InStock { get; set; }

        /// <summary>
        /// Property for Error message
        /// </summary>
        public string ErrorMsg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Attributes { get; set; }

        /// <summary>
        /// Property for crawl date
        /// </summary>
        public string CrawlDate { get; set; }

        ///// <summary>
        ///// Property for file status
        ///// </summary>
        //[DataMember]
        //public ImportStatus FileStatus { get; set; }



        ///// <summary>
        ///// Property for total rows in file
        ///// </summary>
        //[DataMember]
        //public int RowsInFile { get; set; }

        ///// <summary>
        ///// Property for imported rows 
        ///// </summary>
        //[DataMember]
        //public int RowsImported { get; set; }

        ///// <summary>
        ///// Property for error rows count
        ///// </summary>
        //[DataMember]
        //public int RowsWithError { get; set; }

        ///// <summary>
        ///// Property for error file location
        ///// </summary>
        //[DataMember]
        //public string ErrorFilePath { get; set; }

        ///// <summary>
        ///// Property for error file name
        ///// </summary>
        //[DataMember]
        //public string ErrorFileName { get; set; }

        ///// <summary>
        ///// Property for start time
        ///// </summary>
        //[DataMember]
        //public DateTime StartTime { get; set; }

        ///// <summary>
        ///// Property for end time
        ///// </summary>
        //[DataMember]
        //public DateTime EndTime { get; set; }

        ///// <summary>
        ///// Property for total duration 
        ///// </summary>
        //[DataMember]
        //public string Duration { get; set; }

    }

    //[DataContract]
    //public class ImportDataResult
    //{
    //    /// <summary>
    //    /// Property for FileName
    //    /// </summary>
    //    [DataMember]
    //    public string FileName { get; set; }

    //    /// <summary>
    //    /// Property for file format
    //    /// </summary>
    //    [DataMember]
    //    public string Format { get; set; }

    //    /// <summary>
    //    /// Property for total rows in file
    //    /// </summary>
    //    [DataMember]
    //    public int RowsInFile { get; set; }

    //    /// <summary>
    //    /// Property for imported rows 
    //    /// </summary>
    //    [DataMember]
    //    public int RowsImported { get; set; }

    //    /// <summary>
    //    /// Property for error rows count
    //    /// </summary>
    //    [DataMember]
    //    public int RowsWithError { get; set; }

    //    /// <summary>
    //    /// Property for in source file location
    //    /// </summary>
    //    [DataMember]
    //    public string SourceFilePath { get; set; }

    //    /// <summary>
    //    /// Property for error file location
    //    /// </summary>
    //    [DataMember]
    //    public string ErrorFilePath { get; set; }

    //    /// <summary>
    //    /// Property for error file name
    //    /// </summary>
    //    [DataMember]
    //    public string ErrorFileName { get; set; }

    //    /// <summary>
    //    /// Property for start time
    //    /// </summary>
    //    [DataMember]
    //    public DateTime StartTime { get; set; }

    //    /// <summary>
    //    /// Property for end time
    //    /// </summary>
    //    [DataMember]
    //    public DateTime EndTime { get; set; }

    //    /// <summary>
    //    /// Property for total duration 
    //    /// </summary>
    //    [DataMember]
    //    public string Duration { get; set; }
    //    /// <summary>
    //    /// Property for FileName
    //    /// </summary>
    //    [DataMember]
    //    public string Name { get; set; }

    //    /// <summary>
    //    /// Property for file format
    //    /// </summary>
    //    [DataMember]
    //    public string Status { get; set; }

    //    /// <summary>
    //    /// Property for total rows in file
    //    /// </summary>
    //    [DataMember]
    //    public long BatchID { get; set; }

    //    /// <summary>
    //    /// Property for imported rows 
    //    /// </summary>
    //    [DataMember]
    //    public string BatchIdentifier { get; set; }

    //    /// <summary>
    //    /// Property for error rows count
    //    /// </summary>
    //    [DataMember]
    //    public DateTime EffectiveDate { get; set; }

    //    /// <summary>
    //    /// Property for in source file location
    //    /// </summary>
    //    [DataMember]
    //    public DateTime BatchStart { get; set; }

    //    /// <summary>
    //    /// Property for error file location
    //    /// </summary>
    //    [DataMember]
    //    public DateTime BatchEnd { get; set; }

    //    /// <summary>
    //    /// Property for error file name
    //    /// </summary>
    //    [DataMember]
    //    public string ErrorMsg { get; set; }

    //    /// <summary>
    //    /// Property for start time
    //    /// </summary>
    //    [DataMember]
    //    public string StepName { get; set; }
    //}

    //[DataContract]
    //public class CompetitionResult
    //{
    //    /// <summary>
    //    /// Property for FileName
    //    /// </summary>
    //    [DataMember]
    //    public string FileName { get; set; }

    //    /// <summary>
    //    /// Property for file format
    //    /// </summary>
    //    [DataMember]
    //    public string Format { get; set; }

    //    /// <summary>
    //    /// Property for total rows in file
    //    /// </summary>
    //    [DataMember]
    //    public int RowsInFile { get; set; }

    //    /// <summary>
    //    /// Property for imported rows 
    //    /// </summary>
    //    [DataMember]
    //    public int RowsImported { get; set; }

    //    /// <summary>
    //    /// Property for error rows count
    //    /// </summary>
    //    [DataMember]
    //    public int RowsWithError { get; set; }

    //    /// <summary>
    //    /// Property for in source file location
    //    /// </summary>
    //    [DataMember]
    //    public string SourceFilePath { get; set; }

    //    /// <summary>
    //    /// Property for error file location
    //    /// </summary>
    //    [DataMember]
    //    public string ErrorFilePath { get; set; }

    //    /// <summary>
    //    /// Property for error file name
    //    /// </summary>
    //    [DataMember]
    //    public string ErrorFileName { get; set; }

    //    /// <summary>
    //    /// Property for start time
    //    /// </summary>
    //    [DataMember]
    //    public DateTime StartTime { get; set; }

    //    /// <summary>
    //    /// Property for end time
    //    /// </summary>
    //    [DataMember]
    //    public DateTime EndTime { get; set; }

    //    /// <summary>
    //    /// Property for total duration 
    //    /// </summary>
    //    [DataMember]
    //    public string Duration { get; set; }


    //}


    public class ImportError
    {
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string ErrorMessage { get; set; }
    }

}
