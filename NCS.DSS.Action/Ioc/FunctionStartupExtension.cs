using DFC.Common.Standard.CosmosDocumentClient;
using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NCS.DSS.Action.Ioc;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.ServiceBus;
using NCS.DSS.Action.Validation;


[assembly: FunctionsStartup(typeof(FunctionStartupExtension))]

namespace NCS.DSS.Action.Ioc
{
    public class FunctionStartupExtension : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IResourceHelper, ResourceHelper>();
            builder.Services.AddSingleton<IValidate, Validate>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
            builder.Services.AddSingleton<ICosmosDocumentClient, CosmosDocumentClient>();
            builder.Services.AddSingleton<IServiceBusClient, ServiceBusClient>();
            builder.Services.AddSingleton<IGuidHelper, GuidHelper>();

            builder.Services.AddScoped<IActionPatchService, ActionPatchService>();
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddScoped<IGetActionHttpTriggerService, GetActionHttpTriggerService>();
            builder.Services.AddScoped<IGetActionByIdHttpTriggerService, GetActionByIdHttpTriggerService>();
            builder.Services.AddScoped<IPostActionHttpTriggerService, PostActionHttpTriggerService>();
            builder.Services.AddScoped<IPatchActionHttpTriggerService, PatchActionHttpTriggerService>();
        }
    }
}