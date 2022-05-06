using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NUnit.Framework;

namespace NCS.DSS.Action.Tests.FunctionTests
{
    public class GetActionHttpTriggerTest
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private readonly Guid _customerId = Guid.Parse("f5be132b-641e-4b7e-9a95-9d1d8f5aff29");
        private readonly Guid _interactionId = Guid.Parse("3c44cdf0-4f5b-4d6b-ad96-ee1b6001de87");
        private readonly Guid _actionPlanId = Guid.Parse("7fee55ec-9d86-418c-91cd-5e2a3ae3fd6f");

        private Mock<ILogger> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IGetActionHttpTriggerService> _getActionHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IGuidHelper _guidHelper;
        private GetActionHttpTrigger.Function.GetActionHttpTrigger _getActionHttpTrigger;
        private IJsonHelper _jsonHelper;
        private Mock<ILoggerHelper> _loggerHelper;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpRequest(new DefaultHttpContext());
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _loggerHelper = new Mock<ILoggerHelper>();
            _guidHelper = new GuidHelper();
            _log = new Mock<ILogger>();
            _getActionHttpTriggerService = new Mock<IGetActionHttpTriggerService>();

            _getActionHttpTrigger = new GetActionHttpTrigger.Function.GetActionHttpTrigger(
                _resourceHelper.Object,
                _getActionHttpTriggerService.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper);
            //_httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            //_httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            //_resourceHelper.DoesCustomerExist(_customerId).Returns(true);
            //_guidHelper.ValidateGuid(ValidCustomerId).Returns(_customerId);
            //_guidHelper.ValidateGuid(ValidInteractionId).Returns(_interactionId);
            //_guidHelper.ValidateGuid(ValidActionPlanId).Returns(_actionPlanId);

            //_resourceHelper.DoesInteractionExistAndBelongToCustomer(_interactionId, _customerId).Returns(true);
            //_resourceHelper.DoesActionPlanExistAndBelongToCustomer(_actionPlanId, _interactionId, _customerId).Returns(true);
            //SetUpHttpResponseMessageHelper();
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenDsTouchpointIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns<string>(null);
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x=>x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenActionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _getActionHttpTriggerService.Setup(x=>x.GetActionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Action>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeOk_WhenActionExists()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            var listOfActions = new List<Models.Action>();
            _getActionHttpTriggerService.Setup(x=>x.GetActionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(listOfActions));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await _getActionHttpTrigger.Run(
                _request,
                _log.Object,
                customerId,
                interactionId,
                actionPlanId).ConfigureAwait(false);
        }
    }
}