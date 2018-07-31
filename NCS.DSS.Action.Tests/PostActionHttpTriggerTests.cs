using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.Helpers;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.Action.Tests
{
    [TestFixture]
    public class PostActionHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPostActionHttpTriggerService _postActionHttpTriggerService;
        private Models.Action _actionPlan;

        [SetUp]
        public void Setup()
        {
            _actionPlan = Substitute.For<Models.Action>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri =
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/" +
                            $"Interactions/aa57e39e-4469-4c79-a9e9-9cb4ef410382/" +
                            $"Actions")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _validate = Substitute.For<IValidate>();
            _postActionHttpTriggerService = Substitute.For<IPostActionHttpTriggerService>();
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionHasFailedValidation()
        {
            _httpRequestMessageHelper.GetActionFromRequest<Models.Action>(_request).Returns(Task.FromResult(_actionPlan).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Action>(), true).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionRequestIsInvalid()
        {
            _httpRequestMessageHelper.GetActionFromRequest<Models.Action>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetActionFromRequest<Models.Action>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestMessageHelper.GetActionFromRequest<Models.Action>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeOk_WhenActionPlanDoesNotExist()
        {
            _httpRequestMessageHelper.GetActionFromRequest<Models.Action>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateActionRecord()
        {
            _httpRequestMessageHelper.GetActionFromRequest<Models.Action>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(true);

            _postActionHttpTriggerService.CreateAsync(Arg.Any<Models.Action>()).Returns(Task.FromResult<Models.Action>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetActionFromRequest<Models.Action>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(true);

            _postActionHttpTriggerService.CreateAsync(Arg.Any<Models.Action>()).Returns(Task.FromResult<Models.Action>(_actionPlan).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await PostActionHttpTrigger.Function.PostActionHttpTrigger.Run(
                _request, _log, customerId, interactionId, actionPlanId, _resourceHelper, _httpRequestMessageHelper, _validate, _postActionHttpTriggerService).ConfigureAwait(false);
        }

    }
}