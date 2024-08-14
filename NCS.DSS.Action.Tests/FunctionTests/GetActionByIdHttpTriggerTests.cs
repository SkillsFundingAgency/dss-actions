using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;
using GetActionByIdHttpTriggerRun = NCS.DSS.Action.GetActionByIdHttpTrigger.Function.GetActionByIdHttpTrigger;
namespace NCS.DSS.Action.Tests.FunctionTests
{
    [TestFixture]
    public class GetActionByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidActionId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private readonly Guid _customerId = Guid.Parse("1dd4d206-131a-44fd-8e2d-18b88b383f72");
        private readonly Guid _interactionId = Guid.Parse("d800eae2-d594-4eb5-8b23-cf9a39b70f6b");
        private readonly Guid _actionPlanId = Guid.Parse("25cf54a1-f0bb-4590-8c38-191c5329f450");
        private readonly Guid _actionId = Guid.Parse("0f0ce9cd-5f0e-41f7-84e7-6532e01691ae");

        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IGetActionByIdHttpTriggerService> _getActionByIdHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Models.Action _action;
        private IGuidHelper _guidHelper;
        private GetActionByIdHttpTriggerRun _getActionByIdHttpTrigger;

        [SetUp]
        public void Setup()
        {
            _action = new Models.Action();
            _request = (new DefaultHttpContext()).Request;
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            var loggerHelper = new Mock<ILogger<GetActionByIdHttpTriggerRun>>();
            _guidHelper = new GuidHelper();
            _getActionByIdHttpTriggerService = new Mock<IGetActionByIdHttpTriggerService>();
            _getActionByIdHttpTrigger = new GetActionByIdHttpTriggerRun(
                _resourceHelper.Object,
                _getActionByIdHttpTriggerService.Object,
                loggerHelper.Object,
                _httpRequestHelper.Object,
                _guidHelper
                );
        }

        [Test]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(_customerId)).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(_customerId)).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(_interactionId, _customerId)).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenActionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getActionByIdHttpTriggerService.Setup(x => x.GetActionForCustomerAsync(_customerId, _actionId, _actionPlanId)).Returns(Task.FromResult<Models.Action>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeOk_WhenActionExists()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getActionByIdHttpTriggerService.Setup(x => x.GetActionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Action>(_action));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionId, ValidActionPlanId);
            var jsonResult = (JsonResult)result;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionId, string actionPlanId)
        {
            return await _getActionByIdHttpTrigger.Run(
                _request,
                customerId,
                interactionId,
                actionPlanId,
                actionId).ConfigureAwait(false);
        }
    }
}