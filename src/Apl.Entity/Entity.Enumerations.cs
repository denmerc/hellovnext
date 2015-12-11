using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    [DataContract]
    public enum ModuleType
    {
        [EnumMember]
        Null,

        [EnumMember]
        Startup,

        [EnumMember]
        Planning,

        [EnumMember]
        Tracking,

        [EnumMember]
        Reporting,

        [EnumMember]
        Administration
    }

    [DataContract]
    public enum ModuleFeatureType
    {
        [EnumMember]
        None,

        [EnumMember]
        PlanningHome,

        [EnumMember]
        PlanningAnalytics,

        [EnumMember]
        PlanningEverydayPricing,

        [EnumMember]
        PlanningPromotionPricing,

        [EnumMember]
        PlanningKitPricing,

        [EnumMember]
        PlanningCompetition,

        [EnumMember]
        TrackingHome,

        [EnumMember]
        TrackingPerformance,

        [EnumMember]
        TrackingComparison,

        [EnumMember]
        ReportingHome,

        [EnumMember]
        AdminHome,

        [EnumMember]
        AdminUsers,

        [EnumMember]
        AdminTemplates,

        [EnumMember]
        AdminCompetition,

        [EnumMember]
        AdminETLForeignKeyErrors
    }

    [DataContract]
    public enum ModuleFeatureStepType
    {
        [EnumMember]
        None,

        [EnumMember]
        PlanningHomeDashboard,

        [EnumMember]
        PlanningAnalyticsSearch,

        [EnumMember]
        PlanningAnalyticsIdentity,

        [EnumMember]
        PlanningAnalyticsFilters,

        [EnumMember]
        PlanningAnalyticsPriceLists,

        [EnumMember]
        PlanningAnalyticsValueDrivers,

        [EnumMember]
        PlanningAnalyticsResults,

        [EnumMember]
        PlanningPricingSearch,

        [EnumMember]
        PlanningPricingIdentity,

        [EnumMember]
        PlanningPricingFilters,

        [EnumMember]
        PlanningPricingPriceLists,
   
        [EnumMember]
        PlanningPricingRules,      

        [EnumMember]
        PlanningPricingKeyDriver,

        [EnumMember]
        PlanningPricingInfluencers,

        [EnumMember]
        PlanningPricingResults,

        [EnumMember]
        PlanningPricingImpactAnalysis,

        [EnumMember]
        PlanningPricingApproval,

        [EnumMember]
        PlanningKitPricingIdentity,

        [EnumMember]
        PlanningKitPricingFilters,

        [EnumMember]
        PlanningKitPricingPriceLists,

        [EnumMember]
        TrackingHomeDashboard,

        [EnumMember]
        ReportingHomeDashboard,

        [EnumMember]
        AdminHome,

        [EnumMember]
        AdminUserSearch,

        [EnumMember]
        AdminUserIdentity,

        [EnumMember]
        AdminUserCredentials,

        [EnumMember]
        AdminUserRole
    }


    [DataContract]
    public enum PricingResultsEditType
    {
        [EnumMember]
        [Display(Name = "None", Description = "No Edits")]
        None = 0,
        [EnumMember]
        [Display(Name = "Exclude Update", Description = "Exclude Update")]
        ExcludeUpdate,
        //  [EnumMember]
        //  DefaultPrice,
        [EnumMember]
        [Display(Name = "Default To Current Price", Description = "Default To Current Price")]
        DefaultToCurrentPrice,
        [EnumMember]
        [Display(Name = "Default To Maximum Markup", Description = "Default To Maximum Markup")]
        DefaultToMaxMarkup,
        [EnumMember]
        [Display(Name = "Default To Minimum Markup", Description = "Default To Minimum Markup")]
        DefaultToMinMarkup,
        [EnumMember]
        [Display(Name = "Edit New Price", Description = "Edit New Price")]
        EditNewPrice,
        //[EnumMember]
        //EditNewMarkup,
        //[EnumMember]
        //EditRemoveRounding,
        //[EnumMember]
        //EditApplyRounding
    }

    [DataContract]
    public enum PricingResultsWarningType
    {
        [EnumMember]
        [Display(Name = "None", Description = "No Warnings")]
        None = 0,
        [EnumMember]
        [Display(Name = "Markup Below", Description = "Markup Below Minimum Markup")]
        MarkupBelow,
        [EnumMember]
        [Display(Name = "Markup Above", Description = "Markup Above Maximum Markup")]
        MarkupAbove,
        [EnumMember]
        [Display(Name = "Price Optimization Missing", Description = "The corresponding Price Optimization Table has not been assigned.")]
        PriceOptimizationNotPresent,
        [EnumMember]
        [Display(Name = "Price Not Covered", Description = "The optimization price ranges do not cover this price value.")]
        PriceNotCovered,
        [EnumMember]
        [Display(Name = "No Ratio for Global Key+", Description = "Price needed to calculate ratio is not available in target price list.")]
        PriceRatioNotAvailable
    }

    [DataContract]
    public enum UserRoleType
    {
        [EnumMember]
        [Display(Name = "None")]
        None = 0,
        [EnumMember]
        [Display(Name = "Administrator")]
        AplUserRoleAdministrator = 1, // Application (create all, edit own, view all, delete own, approve assigned, schedule all, manage users, manage defaults)
        [EnumMember]
        [Display(Name = "Pricing Analyst")]
        AplUserRolePricingAnalyst = 2, // Application (create price routine, edit own, view own, delete own, approve none, schedule none)
        [EnumMember]
        [Display(Name = "Pricing Approver")]
        AplUserRolePricingApprover = 3, // Application (create price routine, edit own, view own, delete own, approve assigned, schedule none)
        [EnumMember]
        [Display(Name = "Pricing Reviewer")]
        AplUserRolePricingReviewer = 4 // Application (create none, edit none, view all, delete none, approve none, schedule none)
    }

    [DataContract]
    public enum CalculationInterval
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        [Display(Name = "Week")]
        Weekly,

        [EnumMember]
        [Display(Name = "4-Week Period")]
        Period,

        [EnumMember]
        [Display(Name = "Month")]
        Monthly,

        [EnumMember]
        [Display(Name = "Quarter")]
        Quarterly

    }

    [DataContract]
    public enum UnitOfMeasureType
    {
        [EnumMember]
        None,

        [Display(Name = "$")]
        [EnumMember]
        Dollars,

        [Display(Name = "%")]
        [EnumMember]
        Percentage,

        [Display(Name = "Days")]
        [EnumMember]
        Days,

        [Display(Name = "Qty")]
        [EnumMember]
        Quantity,

        [Display(Name = "Units")]
        [EnumMember]
        Units
    }

    [Flags]
    public enum ImportStatus
    {
        [Display(Name = "Transfering file...")]
        Acquiring,

        [Display(Name = "Ready to Import")]
        ReadyToImport,

        [Display(Name = "Validating file...")]
        Preprocessing,

        [Display(Name = "Validation found errors.")]
        FailedPreprocessing,

        [Display(Name = "Adding data to PriceXpert...")]
        Incorporating,

        [Display(Name = "Data Load Complete")]
        Success,

        [Display(Name = "Data Load Failed...")]
        Failed
    }

}

