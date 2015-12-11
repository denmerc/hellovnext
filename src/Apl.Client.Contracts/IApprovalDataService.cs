using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APLPX.Entity;


namespace APLPX.Client.Contracts
{
    public interface IApprovalDataService
    {
        long Submit( PricingApprovalRequest request);
        bool ExecuteAction( long trackingPricingId, ApprovalActionType actionType, int userId, string comments);
        bool Retransmit(long trackingPricingId, int userId, string comments, DateTime retransmitDate);

        List<TrackedPriceRoutine> GetTrackedPriceRoutines(int userId);

        TrackedPriceRoutine GetTrackedPriceRoutine(int trackPricingId);

        //ENT.TrackedPriceRoutine GetTrackedPriceRoutine(int trackPricingId);


    }
}
