using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.Action.Helpers;
using NCS.DSS.Action.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public class ActionPatchService : IActionPatchService
    {
        public string Patch(string actionJson, ActionPatch actionPatch)
        {
            if (string.IsNullOrEmpty(actionJson))
                return null;

            var obj = JObject.Parse(actionJson);

            if (actionPatch.DateActionAgreed.HasValue)
                JsonHelper.UpdatePropertyValue(obj["DateActionAgreed"], actionPatch.DateActionAgreed);

            if (actionPatch.DateActionAimsToBeCompletedBy.HasValue)
                JsonHelper.UpdatePropertyValue(obj["DateActionAimsToBeCompletedBy"], actionPatch.DateActionAimsToBeCompletedBy);

            if (actionPatch.DateActionActuallyCompleted.HasValue)
                JsonHelper.UpdatePropertyValue(obj["DateActionActuallyCompleted"], actionPatch.DateActionActuallyCompleted);

            if (!string.IsNullOrEmpty(actionPatch.ActionSummary))
                JsonHelper.UpdatePropertyValue(obj["ActionSummary"], actionPatch.ActionSummary);

            if (!string.IsNullOrEmpty(actionPatch.SignpostedTo))
                JsonHelper.UpdatePropertyValue(obj["SignpostedTo"], actionPatch.SignpostedTo);

            if (actionPatch.ActionType.HasValue)
                JsonHelper.UpdatePropertyValue(obj["ActionType"], actionPatch.ActionType);

            if (actionPatch.ActionStatus.HasValue)
                JsonHelper.UpdatePropertyValue(obj["ActionStatus"], actionPatch.ActionStatus);

            if (actionPatch.PersonResponsible.HasValue)
                JsonHelper.UpdatePropertyValue(obj["PersonResponsible"], actionPatch.PersonResponsible);

            if (actionPatch.LastModifiedDate.HasValue)
                JsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], actionPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(actionPatch.LastModifiedTouchpointId))
                JsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], actionPatch.LastModifiedTouchpointId);

            if (!string.IsNullOrEmpty(actionPatch.LastModifiedTouchpointId))
                JsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], actionPatch.LastModifiedTouchpointId);
            
            return obj.ToString();
        }
    }
}
