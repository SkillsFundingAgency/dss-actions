using System;

namespace NCS.DSS.Action.PostActionHttpTrigger.Service
{
    public class PostActionHttpTriggerService : IPostActionHttpTriggerService
    {
        public Guid? Create(Models.Action action)
        {
            if (action == null)
                return null;

            return Guid.NewGuid();
        }
    }
}