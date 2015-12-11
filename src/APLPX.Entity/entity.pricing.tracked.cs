using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using APLPX.Entity;

namespace APLPX.Entity
{

    public class TrackedPriceRoutine : PricingEveryday
    {
        public TrackedPriceRoutine() { }

        public long TrackPricingId { get; set; }
        public int TrackAnalyticId { get; set; }
        public string AnalystName { get; set; }
        public string ApproverName { get; set; }
        public DateTime TransmitDate { get; set; }
        public string PriceRoutineType { get; set; }
        public PricingState PricingState { get; set; }
        public List<PricingApprovalEvent> ApprovalEvents { get; set; }
    }

    public class PricingApprovalEvent
    {

        public long EventId { get; set; }
        public long TrackPricingId { get; set; }
        public int UserId { get; set; }
        public int ActionId { get; set; }
        public DateTime EventDate { get; set; }
        public string UserName { get; set; }
        public string PriceRoutineName { get; set; }
        public PricingState PricingStateId { get; set; }
    }
}
