using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetActionPlanFromRequest<T>(HttpRequestMessage req);
    }
}