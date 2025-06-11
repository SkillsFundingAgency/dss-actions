using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using PostActionHttpTriggerRun = NCS.DSS.Action.PostActionHttpTrigger.Function.PostActionHttpTrigger;

namespace NCS.DSS.Action.Tests.FunctionTests
{
    [TestFixture]
    public class PostActionHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidSessionId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ApimUrl = "http://localhost:7001";

        private readonly Guid _customerId = Guid.Parse("b78863da-e7e5-49ae-add5-e1bf1aa6ceb6");

        private Mock<IPostActionHttpTriggerService> _postActionHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILogger<PostActionHttpTriggerRun>> _logger;

        private PostActionHttpTriggerRun _postActionHttpTrigger;
        private IValidate _validate;
        private HttpRequest _request;
        private Models.Action _action;

        [SetUp]
        public void Setup()
        {
            _action = new Models.Action();
            _request = (new DefaultHttpContext()).Request;
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _logger = new Mock<ILogger<PostActionHttpTriggerRun>>();
            _validate = new Validate();
            _postActionHttpTriggerService = new Mock<IPostActionHttpTriggerService>();
            _postActionHttpTrigger = new PostActionHttpTriggerRun(
                _postActionHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                _validate,
                _logger.Object
                );

        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(ApimUrl);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(ApimUrl);

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(ApimUrl);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Action>(_request)).Returns(Task.FromResult(_action));
            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _postActionHttpTrigger = new PostActionHttpTriggerRun(
                _postActionHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                val.Object,
                _logger.Object);

            _postActionHttpTriggerService.Setup(x => x.CreateAsync(_action)).Returns(Task.FromResult<Models.Action>(_action));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(ApimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Action>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(ApimUrl);
            _resourceHelper.Setup(x => x.DoesCustomerExist(_customerId)).Returns(Task.FromResult(false));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Action>(_request)).Returns(Task.FromResult(_action));
            var validationResults = new List<ValidationResult>();
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _postActionHttpTrigger = new PostActionHttpTriggerRun(
                _postActionHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                val.Object,
                _logger.Object);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateActionRecord()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(ApimUrl);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Action>(_request)).Returns(Task.FromResult(_action));
            var validationResults = new List<ValidationResult>();
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _postActionHttpTrigger = new PostActionHttpTriggerRun(
                _postActionHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                val.Object,
                _logger.Object);

            _postActionHttpTriggerService.Setup(x => x.CreateAsync(_action)).Returns(Task.FromResult<Models.Action>(null));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(ApimUrl);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Action>(_request)).Returns(Task.FromResult(_action));
            var validationResults = new List<ValidationResult>();
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _postActionHttpTrigger = new PostActionHttpTriggerRun(
                _postActionHttpTriggerService.Object,
                _httpRequestHelper.Object,
                _resourceHelper.Object,
                val.Object,
                _logger.Object);

            _postActionHttpTriggerService.Setup(x => x.CreateAsync(_action)).Returns(Task.FromResult<Models.Action>(_action));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);
            var jsonResult = (JsonResult)result;

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string sessionId, string actionplanId)
        {
            return await _postActionHttpTrigger.Run(
                _request,
                customerId,
                interactionId,
                actionplanId
               ).ConfigureAwait(false);
        }
    }
}