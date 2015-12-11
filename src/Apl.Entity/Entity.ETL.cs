using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    [DataContract]
    public class FolderStatus
    {

        [DataMember]
        public string StagingFolder { get; set; }
        [DataMember]
        public int InputCount { get; set; }
        [DataMember]
        public int ProcessingCount { get; set; }
        [DataMember]
        public int ErrorCount { get; set; }
        [DataMember]
        public int CompletedCount { get; set; }

    }

    [DataContract]
    public class ProcessSequence
    {

        [DataMember]
        public string StagingFolder { get; set; }
        [DataMember]
        public string ProcessFolderBefore { get; set; }

    }

    [DataContract]
    public class BatchType
    {

        [DataMember]
        public string BatchName { get; set; }
        [DataMember]
        public string Description { get; set; }

    }

    [DataContract]
    public class ETLBatch
    {

        [DataMember]
        public Int64 BatchId { get; set; }
        [DataMember]
        public DateTime EffectiveDate { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DateTime StartTS { get; set; }
        [DataMember]
        public DateTime EndTS { get; set; }

    }

    [DataContract]
    public class ETLBatchStep
    {

        [DataMember]
        public Int64 BatchId { get; set; }
        [DataMember]
        public string StepName { get; set; }
        [DataMember]
        public DateTime StartTS { get; set; }
        [DataMember]
        public DateTime EndTS { get; set; }
        [DataMember]
        public int InputRows { get; set; }
        [DataMember]
        public int RejectedRows { get; set; }
        [DataMember]
        public int AcceptedRows { get; set; }
        [DataMember]
        public char SuccessFlag { get; set; }
        [DataMember]
        public string ErrorMsg { get; set; }

    }


    public class ETLBatchStatus
    {

        public long BatchID { get; set; }

        public string BatchName { get; set; }

        public string BatchIdentifier { get; set; }

        public DateTime BatchStart { get; set; }

        public DateTime BatchEnd { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string Status { get; set; }
        public string StepName { get; set; }
        public string ErrorMsg { get; set; }
        public List<ETLBatchStatusStep> JobSteps { get; set; }

    }

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
    public class ETLValidationError
    {
        public long BatchID { get; set; }
        public string StepName { get; set; }
        public int LineNumber { get; set; }
        public int FieldNumber { get; set; }
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
