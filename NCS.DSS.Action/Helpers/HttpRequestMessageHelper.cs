using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetActionFromRequest<T>(HttpRequestMessage req)
        {
            return await req.Content.ReadAsAsync<T>();
        }
    }
}
