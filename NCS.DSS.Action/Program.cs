using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.ServiceBus;
using NCS.DSS.Action.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.AddSingleton<IResourceHelper, ResourceHelper>();
        services.AddSingleton<IValidate, Validate>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
        services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
        services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
        services.AddSingleton<IJsonHelper, JsonHelper>();
        services.AddSingleton<IConvertToDynamic, ConvertToDynamic>();
        services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
        services.AddSingleton<IServiceBusClient, ServiceBusClient>();
        services.AddSingleton<IGuidHelper, GuidHelper>();
        services.AddScoped<IActionPatchService, ActionPatchService>();
        services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddScoped<IGetActionHttpTriggerService, GetActionHttpTriggerService>();
        services.AddScoped<IGetActionByIdHttpTriggerService, GetActionByIdHttpTriggerService>();
        services.AddScoped<IPostActionHttpTriggerService, PostActionHttpTriggerService>();
        services.AddScoped<IPatchActionHttpTriggerService, PatchActionHttpTriggerService>();
    })
    .Build();

host.Run();
