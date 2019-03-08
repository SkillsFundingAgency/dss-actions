using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class PatchActionHttpTriggerServiceTests
    {
        private IPatchActionHttpTriggerService _actionHttpTriggerService;
        private IActionPatchService _actionPatchService;
        private IDocumentDBProvider _documentDbProvider;
        private string _json;
        private Models.Action _action;
        private ActionPatch _actionPatch;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _actionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _actionPlanId = Guid.Parse("12a16e3f-1c62-1660-3e81-b13122aa81aa");


        [SetUp]
        public void Setup()
        {
            _actionPatchService = Substitute.For<IActionPatchService>();
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _actionHttpTriggerService = Substitute.For<PatchActionHttpTriggerService>(_actionPatchService, _documentDbProvider);
            _actionPatch = Substitute.For<ActionPatch>();
            _action = Substitute.For<Models.Action>();

            _json = JsonConvert.SerializeObject(_actionPatch);
            _actionPatchService.Patch(_json, _actionPatch).Returns(_action.ToString());
        }

        [Test]
        public void PatchActionHttpTriggerServiceTests_PatchResource_ReturnsNullWhenActionJsonIsNullOrEmpty()
        {
            // Act
            var result = _actionHttpTriggerService.PatchResource(null, Arg.Any<ActionPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            _documentDbProvider.UpdateActionAsync(_json, _actionId).ReturnsNull();

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_action.ToString(), _actionId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.CreateActionAsync(Arg.Any<Models.Action>()).Returns(Task.FromResult(new ResourceResponse<Document>(null)).Result);

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_action.ToString(), _actionId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchActionPlanHttpTriggerServiceTests_UpdateAsync_ReturnsResourceWhenUpdated()
        {
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.UpdateActionAsync(_json, _actionId).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _actionHttpTriggerService.UpdateCosmosAsync(_json, _actionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Action>(result);

        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_GetActionForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _documentDbProvider.GetActionForCustomerToUpdateAsync(_customerId, _actionId, _actionPlanId).ReturnsNull();

            // Act
            var result = await _actionHttpTriggerService.GetActionsForCustomerAsync(_customerId, _actionId, _actionPlanId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchActionHttpTriggerServiceTests_GetActionForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _documentDbProvider.GetActionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_action).Result);

            // Act
            var result = await _actionHttpTriggerService.GetActionsForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}