using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.Action.Models;

namespace NCS.DSS.Action.PatchActionHttpTrigger.Service
{
    public interface IActionPatchService
    {
        string Patch(string actionJson, ActionPatch actionPatch);
    }
}
