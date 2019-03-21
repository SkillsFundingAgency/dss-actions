using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace NCS.DSS.Action.ActionChangeFeedTrigger.Service
{
    public interface IActionChangeFeedTriggerService
    {
        Task SendMessageToChangeFeedQueueAsync(Document document);
    }
}
