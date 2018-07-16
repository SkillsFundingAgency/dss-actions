using System;

namespace NCS.DSS.Action.PostActionHttpTrigger.Service
{
    public interface IPostActionHttpTriggerService
    {
        Guid? Create(Models.Action action);
    }
}