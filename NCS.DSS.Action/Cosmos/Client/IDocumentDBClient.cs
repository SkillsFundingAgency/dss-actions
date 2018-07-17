using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Action.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}