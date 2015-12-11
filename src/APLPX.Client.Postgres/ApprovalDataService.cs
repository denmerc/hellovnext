using APLPX.Client.Contracts;
using APLPX.Client.Postgres.Models;
using Newtonsoft.Json;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO = APLPX.Entity;
using DB = APLPX.Client.Postgres.Models;

namespace APLPX.Client.Postgres
{
    public class ApprovalDataService : IApprovalDataService
    {
        private const string EMAIL_SUBJECT_STATUS_TEMPLATE = "Pricing Approval Status change notification for {0}.";
        private const string EMAIL_BODY_STATUS_TEMPLATE = "Approval Pricing Status has changed for <b>\"{0}\"</b> by user, <b>{1}</b>.\n\nThe approval status has changed to <b>{2}</b>.<br><br>";

        //private const string EMAIL_BODY_SUBMIT_TEMPLATE = "Pricing submitted - \"{0}\" by user, {1}.\n\nThe approval status has changed to SUBMITTED.";        
        //private const string EMAIL_SUBJECT_SUBMIT_TEMPLATE = "Pricing submission change notification for Pricing = {0} has been submitted";

        //private const string EMAIL_BODY_RETRANSMIT_TEMPLATE = "Pricing retransmitted - \"{0}\" by user, {1}.\n\nThe approval status has changed to RETRANSMITTED.";
        //private const string EMAIL_SUBJECT_RESTRANSMIT_TEMPLATE = "Pricing retransmission notification for Pricing - {0} has been retransmitted.";
        private OrmLiteConnectionFactory DBConnectionFactory { get; set; }
        private NotificationService NotificationService { get; set; }

        public ApprovalDataService()
        {
            DBConnectionFactory = new OrmLiteConnectionFactory(
                ConfigurationManager.AppSettings["localConnectionString"],
                    PostgreSqlDialect.Provider
                );
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
            NotificationService = new NotificationService();
        }

        public long Submit(Entity.PricingApprovalRequest request)
        {
            var impactAnalyses = request.ImpactAnalyses != null ? JsonConvert.SerializeObject(request.ImpactAnalyses) : "[]";

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var response = db.SqlScalar<int>(string.Format("select \"PX_Track\".\"SP_PricingSubmit\"({0}, {1}, '{2}', {3}, '{4}', '{5}', {6} )", request.UserId, request.PriceRoutineId, request.TransmitTS.ToString("yyyy-MM-dd hh:mm"), request.ApproverId, request.Comments,  impactAnalyses, ((int) request.ApprovalUpdateType)));
                // TODO:dm - scalar<int> a response status?
                if (response != 0 ) { Notify(Entity.ApprovalActionType.Submit, response, request.UserId); }
                return response;
            }
        }

        public bool ExecuteAction(long trackingPricingId, Entity.ApprovalActionType actionType, int userId, string comments)
        {
            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var isSuccess = db.SqlScalar<bool>(string.Format("SELECT \"PX_Track\".\"SP_PricingTakeAction\"({0},{1},{2},'{3}')", userId, trackingPricingId, (int)actionType, comments));

                if (isSuccess) 
                {
                    Notify(actionType, trackingPricingId, userId); 
                };
                return isSuccess;
            }

        }

        public bool Retransmit(long trackingPricingId, int userId, string comments, DateTime transmitDate)
        {
            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                bool isSuccess = db.SqlScalar<bool>(string.Format("SELECT \"PX_Track\".\"SP_PricingRetransmit\"({0},{1},'{2}','{3}')", userId, trackingPricingId, transmitDate, comments ));
                if (isSuccess) { Notify( Entity.ApprovalActionType.Retransmit, trackingPricingId, userId); }
                return isSuccess;
            }
        }


        public List<Entity.TrackedPriceRoutine> GetTrackedPriceRoutines(int userId)
        {

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var tpe = db.Select<DB.TrackPricingExtended>(string.Format("select \"TrackPricingId\" , \"PricingId\" , \"TrackAnalyticId\" , \"Name\" , \"Description\" , \"Notes\" , \"FolderId\" , \"OwnerId\" , \"OwnerName\" ,\"ApproverId\" , \"ApproverName\" ,\"TransmitTS\" , \"PriceRoutineType\" , \"PricingModeId\" , \"PricingStateId\" , \"ProductFilters\" , \"ApprovalEvents\" from \"PX_Track\".\"SP_PricingGetTrackedPriceRoutines\"({0})", userId));
                if (tpe == null) { throw new KeyNotFoundException("TrackedPriceRoutine Not Found."); }


                return tpe
                    .Select( tp => {
                        return new DTO.TrackedPriceRoutine
                        {
                            TrackPricingId = tp.TrackPricingId,
                            TrackAnalyticId = tp.TrackAnalyticId,
                            Id = tp.PricingId,
                            Name = tp.Name,
                            Description = tp.Description,
                            Notes = tp.Notes,
                            FolderId = tp.FolderId,
                            OwnerId = tp.OwnerId,
                            AnalystName = tp.OwnerName,
                            ApproverId = tp.ApproverId,
                            ApproverName =tp.ApproverName,
                            TransmitDate = tp.TransmitTS,
                            PriceRoutineType = tp.PriceRoutineType, 
                            PricingMode = (DTO.PricingMode)tp.PricingModeId,
                            PricingState = (DTO.PricingState)tp.PricingStateId,
                            ApprovalEvents = tp.ApprovalEvents
                                 .Select(e => new DTO.PricingApprovalEvent
                                 {
                                     ActionId = e.ActionId,
                                     EventDate = e.EventTS,
                                     EventId = e.EventId,
                                     PriceRoutineName = tp.Name,
                                     PricingStateId = (DTO.PricingState)tp.PricingStateId,
                                     TrackPricingId = e.TrackPricingId,
                                     UserId = e.UserId,
                                     UserName = e.UserName
                                 }).ToList() 

                        };
                    }).ToList();
            }

        }

        public DTO.TrackedPriceRoutine GetTrackedPriceRoutine(int trackPricingId)
        {
            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                var tp = db.Single<DB.TrackPricingExtended>(string.Format("select \"TrackPricingId\" , \"PricingId\" , \"TrackAnalyticId\" , \"Name\" , \"Description\" , \"Notes\" , \"FolderId\" , \"OwnerId\" , \"OwnerName\" ,\"ApproverId\" , \"ApproverName\" ,\"TransmitTS\" , \"PriceRoutineType\" , \"PricingModeId\" , \"PricingStateId\" , \"ProductFilters\", \"ApprovalEvents\" from \"PX_Track\".\"SP_PricingGetTrackedPriceRoutine\"({0})", trackPricingId));
                //var tp = db.Single<TrackPricing>(p => p.TrackPricingId == trackPricingId);
                if (tp == null) { throw new KeyNotFoundException("TrackedPriceRoutine Not Found."); }
                return new DTO.TrackedPriceRoutine
                {
                    TrackPricingId = tp.TrackPricingId,
                    TrackAnalyticId = tp.TrackAnalyticId,
                    Id = tp.PricingId,
                    Name = tp.Name,
                    Description = tp.Description,
                    Notes = tp.Notes,
                    FolderId = tp.FolderId,
                    OwnerId = tp.OwnerId,
                    AnalystName = tp.OwnerName,
                    ApproverId = tp.ApproverId,
                    ApproverName = tp.ApproverName,
                    TransmitDate = tp.TransmitTS,
                    PriceRoutineType = tp.PriceRoutineType,
                    PricingMode = (DTO.PricingMode)tp.PricingModeId,
                    PricingState = (DTO.PricingState)tp.PricingStateId,
                    Filters = tp.ProductFilters != null ? (from pf in tp.ProductFilters
                                                          group pf by pf.FilterTypeId into g
                                                          select new DTO.FilterGroup
                                                          {
                                                              FilterType = g.Key,
                                                              Filters = g.SelectMany(h => h.Values)
                                                              .Select(fv => new DTO.Filter { Code = fv, FilterType = g.Key }).ToList()
                                                          }).ToList() : new List<DTO.FilterGroup>(),
                    ApprovalEvents = tp.ApprovalEvents
                                 .Select(e => new DTO.PricingApprovalEvent
                                 {
                                     ActionId = e.ActionId,
                                     EventDate = e.EventTS,
                                     EventId = e.EventId,
                                     PriceRoutineName = tp.Name,
                                     PricingStateId = (DTO.PricingState)tp.PricingStateId,
                                     TrackPricingId = e.TrackPricingId,
                                     UserId = e.UserId,
                                     UserName = e.UserName
                                 }).ToList() 
                };
            }
        }

        public void Notify(Entity.ApprovalActionType actionType, long trackPricingId, int actionUserId)
        {
            List<string> addyList = new List<string>();
            string bodyTemplate  = "", subjectTemplate = "";
            TrackPricing tracking;
            int ownerId = 0, approverId = 0;
            string ownerName = "", ownerEmail = "";
            string approverName = "", approverEmail = "";
            string actionUserName = "";

            using (var db = DBConnectionFactory.OpenDbConnection())
            {
                //add actionUser for email template - action not necessarily done by owner
                var actionUser = db.SingleById<User>(actionUserId);
                if (actionUser != null) { actionUserName = string.Format("{0} {1}", actionUser.FirstName, actionUser.LastName); }

                tracking = db.SingleById<TrackPricing>(trackPricingId);
                if (tracking  != null)
                {
                    ownerId = tracking.OwnerId;
                    approverId = tracking.ApproverId;
                }
                var owner = db.SingleById<User>(ownerId);
                if( owner != null )
                { 
                    ownerEmail = owner.Email; 
                    ownerName  = string.Format("{0} {1}", owner.FirstName, owner.LastName);
                }

                var approver = db.SingleById<User>(approverId);
                if( approver != null )
                {
                    approverEmail = approver.Email;
                    approverName = string.Format("{0} {1}", approver.FirstName, approver.LastName);
                }

            }
            switch (actionType)
            {
                case Entity.ApprovalActionType.Submit:
                    //send to approver only
                    addyList.Add(approverEmail);
                    break;
                case Entity.ApprovalActionType.Approve:       
                case Entity.ApprovalActionType.Reject:
                    //send to owner only
                    addyList.Add(ownerEmail);
                    break;
                case Entity.ApprovalActionType.Cancel:
                case Entity.ApprovalActionType.Retransmit:
                case Entity.ApprovalActionType.SystemTransmit:
                case Entity.ApprovalActionType.SystemFail:
                case Entity.ApprovalActionType.RevertPrices:
                case Entity.ApprovalActionType.RevertTransmit:
                    //send to owner and approver
                    addyList.AddRange(new List<string>{ownerEmail, approverEmail});
                    break;
            }
            subjectTemplate = string.Format(EMAIL_SUBJECT_STATUS_TEMPLATE, tracking.Name);
            bodyTemplate = string.Format(EMAIL_BODY_STATUS_TEMPLATE, tracking.Name, actionUserName, actionType.ToString());

            NotificationService.SendEmail(addyList.Distinct().ToList(), subjectTemplate, bodyTemplate, true);
        }
    }
}
