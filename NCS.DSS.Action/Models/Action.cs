using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Action.Models
{
    public class Action
    {
        public Guid ActionId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid ActionPlanId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateActionAgreed { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateActionAimsToBeCompletedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateActionActuallyCompleted { get; set; }

        public string ActionSummary { get; set; }

        public int ActionTypeId { get; set; }

        public int ActionStatusId { get; set; }

        public int PersonResponsibleId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; }

        public Guid LastModifiedBy { get; set; }
    }
}