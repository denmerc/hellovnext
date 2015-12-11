using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;


namespace APLPX.Client.Postgres.Models
{

    [Schema("PX_Stage")]
    public class FolderStatus
    {

        [PrimaryKey]
        [StringLength(50)]
        [Required]
        [References(typeof(BatchType))]
        public string StagingFolder { get; set; }
        
        public int InputCount { get; set; }
        
        public int ProcessingCount { get; set; }
        
        public int ErrorCount { get; set; }
        
        public int CompletedCount { get; set; }

    }

    [Schema("PX_Stage")]
    public class ProcessSequence
    {

        [PrimaryKey]
        [StringLength(50)]
        [Required]
        [References(typeof(BatchType))]
        public string StagingFolder { get; set; }

        [PrimaryKey]
        [StringLength(50)]
        [Required]
        [References(typeof(BatchType))]
        public string ProcessFolderBefore { get; set; }

    }

    [Schema("PX_Stage")]
    public class BatchType
    {

        [PrimaryKey]
        [StringLength(50)]
        [Required]
        public string BatchName { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

    }

    [Schema("PX_Stage")]
    public class ETLBatch
    {

        [PrimaryKey]
        [AutoIncrement]
        public long BatchID { get; set; } 
        
        public DateTime EffectiveDate { get; set; }
 
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public string BatchIdentifier { get; set; }

        [Required]
        public DateTime StartTS { get; set; }

        public DateTime EndTS { get; set; }

    }


    [Schema("PX_Stage")]
    public class ETLBatchStatus
    {

        [PrimaryKey]
        [AutoIncrement]
        public long BatchID { get; set; } 

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public string BatchIdentifier { get; set; }

        public DateTime BatchStart { get; set; }

        public DateTime BatchEnd { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string Status { get; set; }
        public string StepName { get; set; }
        public string ErrorMsg { get; set; }


    }


    [Schema("PX_Stage")]
    public class ETLBatchStep
    {

        [PrimaryKey]
        public long BatchID { get; set; } 

        [PrimaryKey]
        [StringLength(50)]
        public string StepName { get; set; }
        
        [Required]
        public DateTime StartTS { get; set; }
        
        public DateTime EndTS { get; set; }
        
        public int InputRows { get; set; }
        
        public int RejectedRows { get; set; }
        
        public int AppliedRows { get; set; }
        
        public char SuccessFlag { get; set; }
        
        [StringLength(250)]
        public string ErrorMsg { get; set; }

    }

    [Schema("information_schema")]
    public class PGColumnInfo
    {
        public string column_name { get; set; }
        public int ordinal_position { get; set; }
        public string is_nullable { get; set; }
        public string udt_name { get; set; }
        public int character_maximum_length { get; set; }
    }


    [Schema("PX_Stage")]
    public class BatchStoredProcedure : BatchJob
    {

        public string StoredProcedureName { get; set; }

    }

    [Schema("PX_Stage")]
    public class BatchServerCommand : BatchJob
    {

        public string ServerCommand { get; set; }

    }

    public class BatchJob
    {
        [PrimaryKey]
        public string BatchName { get; set; }

        [Required]
        public string ScheduleFrequencyParameter { get; set; }
        public string ScheduleTimeParameter { get; set; }
        public string ScheduleDOWParameter { get; set; }
        public string ScheduleMonthDayParameter { get; set; }
    }

    [Schema ("PX_Stage")]
    public class ExecutionQueue
    {
        public long BatchID { get; set; }
        public string StoredProcedureName { get; set; }
    }

    [Schema("PX_Stage")]
    [Alias("V_ETLJobStatusRecent")]
    public class ETLRecentJobStatus
    {
        public string BatchName { get; set; }
        public string BatchIdentifier { get; set; }
        public DateTime BatchStart { get; set; }
        public DateTime BatchEnd { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Status { get; set; }  
        public string LastStep { get; set; }
        public string LastStepError { get; set; }
        public long BatchID { get; set; }
    }

    [Schema("PX_Stage")]
    [Alias("V_ETLJobStatusSteps")]
    public class ETLBatchStatusStep
    {
        public long BatchID { get; set; }
        public string StepName { get; set; }
        public string Status { get; set; }
        public DateTime StepStart { get; set; }
        public DateTime StepEnd { get; set; }
        public string StepError { get; set; }
        public int InputRowCount { get; set; }
        public int RejectedRowCount { get; set; }
        public int AppliedRowCount { get; set; }
        public long NumValidationErrors { get; set; }
    }

    [Schema("PX_Stage")]
    public class ETLValidationError
    {
        public long BatchID {get;set;}
        public string StepName {get;set;}
        public int LineNumber {get;set;}
        public int FieldNumber {get;set;}
        public string FileName {get;set;}
        public string ErrorMessage {get;set;}
     }
 
}
