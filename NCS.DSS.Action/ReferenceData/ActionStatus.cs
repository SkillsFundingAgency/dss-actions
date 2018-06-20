
using System.ComponentModel;

namespace NCS.DSS.Action.ReferenceData
{
    public enum ActionStatus
    {
        [Description("Not Started")]
        NotStarted = 1,

        [Description("In Progress")]
        InProgress = 2,

        Completed = 3,

        [Description("No longer applicable")]
        NoLongerApplicable = 99
    }
}
