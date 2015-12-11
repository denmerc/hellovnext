using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    [DataContract]
    public class Module //Workflow groups
    {
        

        [DataMember]
        public ModuleType Type { get;  set; }
        [DataMember]
        public string Name { get;  set; }
        [DataMember]
        public string Title { get;  set; }
        [DataMember]
        public short Sort { get;  set; }
        [DataMember]
        public List<ModuleFeature> Features { get;  set; }
    }

    [DataContract]
    public class ModuleFeature  //Workflow Views
    {


        [DataMember]
        public ModuleFeatureType Type { get; set; }
        //[DataMember]
        //public ModuleFeatureStepType LandingStepType { get;  set; }
        //[DataMember]
        //public ModuleFeatureStepType ActionStepType { get;  set; }
        [DataMember]
        public string Name { get;  set; }
        [DataMember]
        public string Description { get;  set; }
        //[DataMember]
        //public short Sort { get;  set; }
        //[DataMember]
        //public List<ModuleFeatureStep> Steps { get;  set; }
        [DataMember]
        public string[] SearchGroups { get;  set; }
    }

    //[DataContract]
    //public class ModuleFeatureStep //Workflow View Steps
    //{
        

    //    [DataMember]
    //    public ModuleFeatureStepType Type { get;  set; }
    //    [DataMember]
    //    public string Name { get;  set; }
    //    [DataMember]
    //    public string Title { get;  set; }
    //    [DataMember]
    //    public short Sort { get;  set; }
    //    [DataMember]
    //    public List<ModuleFeatureStepAction> Actions { get;  set; }
    //    [DataMember]
    //    public List<ModuleFeatureStepAdvisor> Advisors { get;  set; }
    //    [DataMember]
    //    public List<ModuleFeatureStepError> Errors { get;  set; }
    //}

    //[DataContract]
    //public class ModuleFeatureStepAction
    //{
        

    //    [DataMember]
    //    public string Name { get;  set; }
    //    [DataMember]
    //    public string ParentName { get;  set; }
    //    [DataMember]
    //    public string Title { get;  set; }
    //    [DataMember]
    //    public short Sort { get;  set; }
    //    [DataMember]
    //    public ModuleFeatureStepActionType Type { get;  set; }
    //}

    //[DataContract]
    //public class ModuleFeatureStepAdvisor
    //{
        

    //    [DataMember]
    //    public short Sort { get;  set; }
    //    [DataMember]
    //    public string Message { get;  set; }
    //}

    //[DataContract]
    //public class ModuleFeatureStepError
    //{
        

    //    [DataMember]
    //    public short Sort { get;  set; }
    //    [DataMember]
    //    public string Message { get; set; }
    //}

    //[DataContract]
    //public class FeatureSearchGroup
    //{
        

    //    [DataMember]
    //    public int SearchGroupId { get;  set; }
    //    [DataMember]
    //    public string SearchGroupKey { get;  set; }
    //    [DataMember]
    //    public string Name { get; set; }
    //    [DataMember]
    //    public string ParentName { get;  set; }
    //    [DataMember]
    //    public bool CanNameChange { get;  set; }
    //    [DataMember]
    //    public bool CanSearchGroupChange { get;  set; }
    //    [DataMember]
    //    public short ItemCount { get; set; }
    //    [DataMember]
    //    public bool IsNameChanged { get; set; }
    //    [DataMember]
    //    public bool IsSearchGroupChanged { get; set; }
    //    [DataMember]
    //    public short Sort { get;  set; }
    //}
}
