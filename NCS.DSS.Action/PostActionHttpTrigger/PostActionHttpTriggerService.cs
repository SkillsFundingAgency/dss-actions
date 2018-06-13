using System;

namespace NCS.DSS.Action.PostActionHttpTrigger
{
    public class PostActionHttpTriggerService
    {
        public Guid? Create(Models.Action action)
        {
            if (action == null)
                return null;

            return Guid.NewGuid();
        }
    }
}