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

namespace NCS.DSS.Action.Tests.FunctionTests
{
    public class GetActionHttpTriggerTest
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private Mock<IGetActionHttpTriggerService> _getActionHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILogger<GetActionHttpTrigger.Function.GetActionHttpTrigger>> _logger;

        private GetActionHttpTrigger.Function.GetActionHttpTrigger _getActionHttpTrigger;
        private HttpRequest _request;

        [SetUp]
        public void Setup()
        {
            _getActionHttpTriggerService = new Mock<IGetActionHttpTriggerService>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _resourceHelper = new Mock<IResourceHelper>();
            _logger = new Mock<ILogger<GetActionHttpTrigger.Function.GetActionHttpTrigger>>();

            _getActionHttpTrigger = new GetActionHttpTrigger.Function.GetActionHttpTrigger(
                _getActionHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                _logger.Object);

            _request = new DefaultHttpContext().Request;
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
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(false));

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
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
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
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
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