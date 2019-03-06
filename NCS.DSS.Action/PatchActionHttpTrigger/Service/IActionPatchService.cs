using NCS.DSS.Action.Models;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public interface IActionPatchService
    {
        string Patch(string actionJson, ActionPatch actionPatch);
    }
}
