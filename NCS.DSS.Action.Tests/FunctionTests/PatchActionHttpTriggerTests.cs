﻿using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.PatchActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using PatchActionHttpTriggerRun = NCS.DSS.Action.PatchActionHttpTrigger.Function.PatchActionHttpTrigger;
namespace NCS.DSS.Action.Tests.FunctionTests
{
    [TestFixture]
    public class PatchActionHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private readonly Guid _customerId = Guid.Parse("01b9d326-ca0f-42f4-a164-61746c26b079");
        private readonly Guid _interactionId = Guid.Parse("2ff59239-991d-4fc6-84eb-dcbe87fc8730");
        private readonly Guid _actionPlanId = Guid.Parse("d48ddb20-809f-4fb1-ade2-72e9cbd4ec7f");
        private readonly Guid _actionId = Guid.Parse("05c85a2d-af4d-4beb-846b-b046a04e094a");
        private readonly string _apimUrl = "https://loocalhost:7001";

        private readonly HttpRequest _request;
        private readonly Mock<IResourceHelper> _resourceHelper;
        private readonly IValidate _validate;
        private readonly Mock<IPatchActionHttpTriggerService> _patchActionHttpTriggerService;
        private readonly Mock<IHttpRequestHelper> _httpRequestHelper;
        private readonly Models.Action _action;
        private readonly ActionPatch _actionPatch;
        private readonly string _actionString = string.Empty;
        private PatchActionHttpTriggerRun _patchActionHttpTrigger;
        private readonly IGuidHelper _guidHelper;
        private Mock<ILogger<PatchActionHttpTriggerRun>> _loggerHelper;
        private IConvertToDynamic _convertToDynamic;

        public PatchActionHttpTriggerTests()
        {
            _action = new Models.Action();
            _actionPatch = new ActionPatch() { ActionType = ReferenceData.ActionType.ApplyForApprenticeship, DateActionAgreed = DateTime.Now, ActionStatus = ReferenceData.ActionStatus.InProgress };
            _request = (new DefaultHttpContext()).Request;
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _loggerHelper = new Mock<ILogger<PatchActionHttpTriggerRun>>();
            _convertToDynamic = new ConvertToDynamic();
            _guidHelper = new GuidHelper();

            _validate = new Validate();
            _patchActionHttpTriggerService = new Mock<IPatchActionHttpTriggerService>();
            _patchActionHttpTrigger = new PatchActionHttpTriggerRun(
                _resourceHelper.Object,
                _patchActionHttpTriggerService.Object,
                _loggerHelper.Object,
                _validate,
                _httpRequestHelper.Object,
                _convertToDynamic,
                _guidHelper);

            _actionString = JsonConvert.SerializeObject(_action);
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }


        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<ActionPatch>(_request)).Returns(Task.FromResult(_actionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchActionHttpTriggerService.Setup(x => x.GetActionsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(_actionString));
            _patchActionHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_action));
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchActionHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<ActionPatch>())).Returns(_actionString);
            var validationResults = new List<ValidationResult> { new ValidationResult("Please supply a valid Action Type") };
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _patchActionHttpTrigger = new PatchActionHttpTriggerRun(
                _resourceHelper.Object,
                _patchActionHttpTriggerService.Object,
                _loggerHelper.Object,
                val.Object,
                _httpRequestHelper.Object,
                _convertToDynamic,
                _guidHelper);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<ActionPatch>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<ActionPatch>(_request)).Returns(Task.FromResult(_actionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(_customerId)).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<ActionPatch>(_request)).Returns(Task.FromResult(_actionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeNoContent_WhenActionDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<ActionPatch>(_request)).Returns(Task.FromResult(_actionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            _patchActionHttpTriggerService.Setup(x => x.GetActionsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<ActionPatch>(_request)).Returns(Task.FromResult(_actionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchActionHttpTriggerService.Setup(x => x.GetActionsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(_actionString));
            _patchActionHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_action));
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchActionHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<ActionPatch>())).Returns(_actionString);
            var val = new Mock<IValidate>();
            _patchActionHttpTrigger = new PatchActionHttpTriggerRun(
                _resourceHelper.Object,
                _patchActionHttpTriggerService.Object,
                _loggerHelper.Object,
                val.Object,
                _httpRequestHelper.Object,
                _convertToDynamic,
                _guidHelper);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidActionId);
            var jsonResult = (JsonResult)result;
            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        [Test]
        public async Task PatchActionHttpTrigger_ReturnsStatusCodeNoContent_WhenActionPlanCannotBeFound()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<ActionPatch>(_request)).Returns(Task.FromResult(_actionPatch));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchActionHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_action));
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _patchActionHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<ActionPatch>())).Returns(_actionString);
            _patchActionHttpTriggerService.Setup(x => x.GetActionsForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidActionId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }


        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionPlanId, string actionId)
        {
            return await _patchActionHttpTrigger.Run(
                _request,
                customerId,
                interactionId,
                actionPlanId,
                actionId).ConfigureAwait(false);
        }
    }
}