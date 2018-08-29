using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Action.Annotations;
using NCS.DSS.Action.ReferenceData;

namespace NCS.DSS.Action.Models
{
    public class ActionPatch : IAction
    {
        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action plan was agreed with the customer.")]
        [Example(Description = "2018-06-21T07:20:00")]
        public DateTime? DateActionAgreed { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action should be completed, with by the customer or the adviser.")]
        [Example(Description = "2018-06-27T09:00:00")]
        public DateTime? DateActionAimsToBeCompletedBy { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the action was completed by the customer or the adviser.")]
        [Example(Description = "2018-06-24T14:38:00")]
        public DateTime? DateActionActuallyCompleted { get; set; }

        [StringLength(4000)]
        [Display(Description = "Summary of the action.")]
        [Example(Description = "this is some text")]
        public string ActionSummary { get; set; }

        [StringLength(255)]
        [Display(Description = "Details of any signposting to external parties.")]
        [Example(Description = "this is some text")]
        public string SignpostedTo { get; set; }

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
        }
    }
}