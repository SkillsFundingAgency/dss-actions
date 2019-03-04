using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NCS.DSS.Action.Helpers;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;


namespace NCS.DSS.Action.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddTransient<IGetActionHttpTriggerService, GetActionHttpTriggerService>();
            services.AddTransient<IGetActionByIdHttpTriggerService, GetActionByIdHttpTriggerService>();
            services.AddTransient<IPostActionHttpTriggerService, PostActionHttpTriggerService>();
            services.AddTransient<IPatchActionHttpTriggerService, PatchActionHttpTriggerService>();
            services.AddScoped<IActionPatchService, ActionPatchService>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();

            return services.BuildServiceProvider(true);
        }
    }
}
