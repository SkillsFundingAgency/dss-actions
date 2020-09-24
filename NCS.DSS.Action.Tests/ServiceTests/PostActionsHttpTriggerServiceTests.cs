using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Action.Cosmos.Provider;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Action.Tests.ServiceTests
{
    [TestFixture]
    public class PostActionHttpTriggerServiceTests
    {
        private IPostActionHttpTriggerService _actionHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private string _json;
        private Models.Action _action;
        private readonly Guid _actionId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _actionHttpTriggerService = Substitute.For<PostActionHttpTriggerService>(_documentDbProvider);
            _action = Substitute.For<Models.Action>();
            _json = JsonConvert.SerializeObject(_action);
        }

        [Test]
        public async Task PostActionHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenActionJsonIsNull()
        {
            // Act
            var result = await _actionHttpTriggerService.CreateAsync(Arg.Any<Models.Action>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PostActionHttpTriggerServiceTests_CreateAsync_ReturnsResource()
        {
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.Created, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.CreateActionAsync(Arg.Any<Models.Action>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _actionHttpTriggerService.CreateAsync(_action);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Action>(result);

        }
    }
}