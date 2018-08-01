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

        [Required]
        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [Required]
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
        [Display(Description = "ActionType reference data.")]
        [Example(Description = "1")]
        public ActionType? ActionType { get; set; }

        [Display(Description = "ActionStatus reference data.")]
        [Example(Description = "1")]
        public ActionStatus? ActionStatus { get; set; }

        [Required]
        [Display(Description = "PersonResponsible reference data.")]
        [Example(Description = "1")]
        public PersonResponsible? PersonResponsible { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-20T13:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "d1307d77-af23-4cb4-b600-a60e04f8c3df")]
        public Guid? LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
            ActionId = Guid.NewGuid();

            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;

            if (ActionStatus == null)
                ActionStatus = ReferenceData.ActionStatus.NotStarted;
        }

        public void Patch(ActionPatch actionPatch)
        {
            if (actionPatch == null)
                return;

            DateActionAgreed = actionPatch.DateActionAgreed;

            DateActionAimsToBeCompletedBy = actionPatch.DateActionAimsToBeCompletedBy;

            DateActionActuallyCompleted = actionPatch.DateActionActuallyCompleted;

            ActionSummary = actionPatch.ActionSummary;

            SignpostedTo = actionPatch.SignpostedTo;

            ActionType = actionPatch.ActionType;


            ActionStatus = actionPatch.ActionStatus;

            PersonResponsible = actionPatch.PersonResponsible;
            
            LastModifiedDate = actionPatch.LastModifiedDate;

            LastModifiedTouchpointId = actionPatch.LastModifiedTouchpointId;
        }
    }
}