using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Action.Annotations;
using NCS.DSS.Action.ReferenceData;

namespace NCS.DSS.Action.Models
{
    public class Action : IAction
    {
        [Display(Description = "Unique identifier for a action record")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? ActionId { get; set; }

        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [Display(Description = "Unique identifier of the customer action plan.")]
        [Example(Description = "a79ba4cc-5da5-4eb0-8913-eb5e69f90ab8")]
        public Guid? ActionPlanId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action plan was agreed with the customer.")]
        [Example(Description = "2018-06-21T07:20:00")]
        public DateTime? DateActionAgreed { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action should be completed, with by the customer or the adviser.")]
        [Example(Description = "2018-06-27T09:00:00")]
        public DateTime? DateActionAimsToBeCompletedBy { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action was completed by the customer or the adviser.")]
        [Example(Description = "2018-06-24T14:38:00")]
        public DateTime? DateActionActuallyCompleted { get; set; }

        [Required]
        [StringLength(4000)]
        [Display(Description = "Summary of the action.")]
        [Example(Description = "this is some text")]
        public string ActionSummary { get; set; }

        [StringLength(255)]
        [Display(Description = "Details of any signposting to external parties.")]
        [Example(Description = "this is some text")]
        public string SignpostedTo { get; set; }

        [Required]
        [Display(Description = "ActionType reference data </br>" +
                                "1 - Skills Health Check </br>" +
                                "2 - Create or update CV </br>" +
                                "3 - Interview skills workshop </br>" +
                                "4 - Search for vacancy </br>" +
                                "5 - Enrol on a course </br>" +
                                "6 - Careers management workshop </br>" +
                                "7 - Apply for apprenticeship </br>" +
                                "8 - Apply for traineeship </br>" +
                                "9 - Attend skills fair or skills show </br>" +
                                "10 - Volunteer </br>" +
                                "11 - Use National Careers Service website </br>" +
                                "12 - Use external digital services </br>" +
                                "13 - Book follow up appointment </br>" +
                                "14 - Use social media </br>")]
        [Example(Description = "1")]
        public ActionType? ActionType { get; set; }

        [Display(Description = "ActionStatus reference data." +
                                "1 - Not Started </br>" +
                                "2 - In Progress </br>" +
                                "3 - Completed </br>" +
                                "99 - No longer applicable")]
        [Example(Description = "1")]
        public ActionStatus? ActionStatus { get; set; }

        [Required]
        [Display(Description = "PersonResponsible reference data. </br>" +
                                "1 - Customer </br>" + 
                                "2 - Adviser")]
        [Example(Description = "1")]
        public PersonResponsible? PersonResponsible { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;

            if (ActionStatus == null)
                ActionStatus = ReferenceData.ActionStatus.NotStarted;
        }

        public void SetIds(Guid customerId, Guid actionPlanId, string touchpointId)
        {
            ActionId = Guid.NewGuid();
            CustomerId = customerId;
            ActionPlanId = actionPlanId;
            LastModifiedTouchpointId = touchpointId;
        }

        public void Patch(ActionPatch actionPatch)
        {
            if (actionPatch == null)
                return;

            if(actionPatch.DateActionAgreed.HasValue)
                DateActionAgreed = actionPatch.DateActionAgreed;

            if(actionPatch.DateActionAimsToBeCompletedBy.HasValue)
                DateActionAimsToBeCompletedBy = actionPatch.DateActionAimsToBeCompletedBy;

            if(actionPatch.DateActionActuallyCompleted.HasValue)
                DateActionActuallyCompleted = actionPatch.DateActionActuallyCompleted;

            if(!string.IsNullOrWhiteSpace(actionPatch.ActionSummary))
                ActionSummary = actionPatch.ActionSummary;

            if(!string.IsNullOrWhiteSpace(actionPatch.SignpostedTo))
                SignpostedTo = actionPatch.SignpostedTo;

            if(actionPatch.ActionType.HasValue)
                ActionType = actionPatch.ActionType;

            if(actionPatch.ActionStatus.HasValue)
                ActionStatus = actionPatch.ActionStatus;

            if(actionPatch.PersonResponsible.HasValue)
                PersonResponsible = actionPatch.PersonResponsible;
            
            if(actionPatch.LastModifiedDate.HasValue)
                LastModifiedDate = actionPatch.LastModifiedDate;

            if(!string.IsNullOrEmpty(actionPatch.LastModifiedTouchpointId))
                LastModifiedTouchpointId = actionPatch.LastModifiedTouchpointId;
        }

    }
}