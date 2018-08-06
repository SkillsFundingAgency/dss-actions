using System;
using NCS.DSS.Action.ReferenceData;

namespace NCS.DSS.Action.Models
{
    public interface IAction
    {
        DateTime? DateActionAgreed { get; set; }
        DateTime? DateActionAimsToBeCompletedBy { get; set; }
        DateTime? DateActionActuallyCompleted { get; set; }
        string ActionSummary { get; set; }
        string SignpostedTo { get; set; }
        ActionType? ActionType { get; set; }
        ActionStatus? ActionStatus { get; set; }
        PersonResponsible? PersonResponsible { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();

    }
}