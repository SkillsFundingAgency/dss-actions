﻿using System.Net;
using System.Net.Http;
using System.Reflection;
using DFC.Functions.DI.Standard.Attributes;
using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace NCS.DSS.Action.APIDefinition
{
    public static class GenerateActionSwaggerDoc
    {
        public const string ApiTitle = "Actions";
        public const string ApiDefinitionName = "API-Definition";
        public const string ApiDefRoute = ApiTitle + "/" + ApiDefinitionName;
        public const string ApiDescription = "To support the Data Collections integration with DSS SubcontractorId has been added as an attribute.";
        public const string ApiVersion = "2.0.0";

        [FunctionName(ApiDefinitionName)]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiDefRoute)]HttpRequest req,
            [Inject]ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            var swagger = swaggerDocumentGenerator.GenerateSwaggerDocument(req, ApiTitle, ApiDescription, ApiDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swagger)
            };
        }
    }
}