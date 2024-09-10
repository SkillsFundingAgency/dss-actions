using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GetActionHttpTriggerRun = NCS.DSS.Action.GetActionHttpTrigger.Function.GetActionHttpTrigger;

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


        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IGetActionHttpTriggerService> _getActionHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IGuidHelper _guidHelper;
        private GetActionHttpTriggerRun _getActionHttpTrigger;
        private Mock<ILogger<GetActionHttpTriggerRun>> _loggerHelper;

        [SetUp]
        public void Setup()
        {
            _request = (new DefaultHttpContext()).Request;
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _loggerHelper = new Mock<ILogger<GetActionHttpTriggerRun>>();
            _guidHelper = new GuidHelper();
            _getActionHttpTriggerService = new Mock<IGetActionHttpTriggerService>();

            _getActionHttpTrigger = new GetActionHttpTriggerRun(
                _resourceHelper.Object,
                _getActionHttpTriggerService.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _guidHelper);

        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenDsTouchpointIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns<string>(null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenActionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _getActionHttpTriggerService.Setup(x => x.GetActionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Action>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeOk_WhenActionExists()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            var listOfActions = new List<Models.Action>();
            _getActionHttpTriggerService.Setup(x => x.GetActionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(listOfActions));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);
            var jsonResult = (JsonResult)result;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await _getActionHttpTrigger.Run(
                _request,
                customerId,
                interactionId,
                actionPlanId).ConfigureAwait(false);
        }
    }
}