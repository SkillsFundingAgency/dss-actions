using System.Dynamic;

namespace NCS.DSS.Action.Cosmos.Helper
{
    public interface IDynamicHelper
    {
        public ExpandoObject ExcludeProperty(Exception exception, string[] names);

    }
}
