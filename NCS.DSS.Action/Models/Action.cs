using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Action.ReferenceData;

namespace NCS.DSS.Action.Models
{
    public class Action
    {
        [Display(Description = "Unique identifier for a action record")]
        public Guid ActionId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a customer.")]
        public Guid CustomerId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of the customer action plan.")]
        public Guid ActionPlanId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action plan was agreed with the customer.")]
        public DateTime DateActionAgreed { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action should be completed, with by the customer or the adviser.")]
        public DateTime DateActionAimsToBeCompletedBy { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action was completed by the customer or the adviser.")]
        public DateTime DateActionActuallyCompleted { get; set; }

        [Required]
        [StringLength(4000)]
        [Display(Description = "Summary of the action.")]
        public string ActionSummary { get; set; }

        [StringLength(255)]
        [Display(Description = "Details of any signposting to external parties.")]
        public string SignpostedTo { get; set; }

        [Required]
        [Display(Description = "ActionType reference data.")]
        public int ActionType { get; set; }

        [Display(Description = "ActionStatus reference data.")]
        public ActionStatus ActionStatus { get; set; }

        [Required]
        [Display(Description = "PersonResponsible reference data.")]
        public PersonResponsible PersonResponsible { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}