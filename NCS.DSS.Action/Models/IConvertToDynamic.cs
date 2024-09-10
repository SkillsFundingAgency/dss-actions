using System.Dynamic;

namespace NCS.DSS.Action.Models
{
    public interface IConvertToDynamic
    {
        public ExpandoObject ExcludeProperty(Exception exception, string[] names);

    }
}
