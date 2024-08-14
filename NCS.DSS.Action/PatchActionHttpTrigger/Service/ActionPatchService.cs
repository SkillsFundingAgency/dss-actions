using DFC.JSON.Standard;
using NCS.DSS.Action.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public class ActionPatchService : IActionPatchService
    {
        private readonly IJsonHelper _jsonHelper;

        public ActionPatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }

        public string Patch(string actionJson, ActionPatch actionPatch)
        {
            if (string.IsNullOrEmpty(actionJson))
                return null;

            var obj = JObject.Parse(actionJson);

            if (actionPatch.DateActionAgreed.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateActionAgreed"], actionPatch.DateActionAgreed);

            if (actionPatch.DateActionAimsToBeCompletedBy.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateActionAimsToBeCompletedBy"], actionPatch.DateActionAimsToBeCompletedBy);

            if (actionPatch.DateActionActuallyCompleted.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateActionActuallyCompleted"], actionPatch.DateActionActuallyCompleted);

            if (!string.IsNullOrEmpty(actionPatch.ActionSummary))
                _jsonHelper.UpdatePropertyValue(obj["ActionSummary"], actionPatch.ActionSummary);

            if (!string.IsNullOrEmpty(actionPatch.SignpostedTo))
                _jsonHelper.UpdatePropertyValue(obj["SignpostedTo"], actionPatch.SignpostedTo);

            if (actionPatch.SignpostedToCategory.HasValue)
            {
                if (obj["SignpostedToCategory"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SignpostedToCategory", actionPatch.SignpostedToCategory);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SignpostedToCategory"], actionPatch.SignpostedToCategory);
            }

            if (actionPatch.ActionType.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["ActionType"], actionPatch.ActionType);

            if (actionPatch.ActionStatus.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["ActionStatus"], actionPatch.ActionStatus);

            if (actionPatch.PersonResponsible.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["PersonResponsible"], actionPatch.PersonResponsible);

            if (actionPatch.LastModifiedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], actionPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(actionPatch.LastModifiedTouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], actionPatch.LastModifiedTouchpointId);

            if (!string.IsNullOrEmpty(actionPatch.LastModifiedTouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], actionPatch.LastModifiedTouchpointId);

            return obj.ToString();
        }
    }
}
