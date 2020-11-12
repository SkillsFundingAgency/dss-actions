using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
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
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Tests.FunctionTests
{
    [TestFixture]
    public class PostActionHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidSessionId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private readonly Guid _customerId = Guid.Parse("b78863da-e7e5-49ae-add5-e1bf1aa6ceb6");
        private readonly Guid _interactionId = Guid.Parse("0f0ce9cd-5f0e-41f7-84e7-6532e01691ae");
        private readonly Guid _actionPlanId = Guid.Parse("f03516d5-7644-4daf-b713-7b6e62167939");
        private readonly string _apimUrl = "http://localhost:7001";

        private Mock<ILogger> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private IValidate _validate;
        private Mock<IPostActionHttpTriggerService> _postActionHttpTriggerService;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IGuidHelper _guidHelper;
        private Models.Action _action;
        private PostActionHttpTrigger.Function.PostActionHttpTrigger _postActionHttpTrigger;
        private Mock<ILoggerHelper> _loggerHelper;
        private IJsonHelper _jsonHelper;

        [SetUp]
        public void Setup()
        {
            _action = new Models.Action();
            _request = new DefaultHttpRequest(new DefaultHttpContext());
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _loggerHelper = new Mock<ILoggerHelper>();
            _guidHelper = new GuidHelper();
            _validate = new Validate();
            _log = new Mock<ILogger>();
            _postActionHttpTriggerService = new Mock<IPostActionHttpTriggerService>();
            _postActionHttpTrigger = new PostActionHttpTrigger.Function.PostActionHttpTrigger(_resourceHelper.Object,
                _postActionHttpTriggerService.Object,
                _loggerHelper.Object,
                _validate,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x=>x.GetDssApimUrl(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Action>(_request)).Returns(Task.FromResult(_action));
            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _postActionHttpTrigger = new PostActionHttpTrigger.Function.PostActionHttpTrigger(_resourceHelper.Object,
                _postActionHttpTriggerService.Object,
                _loggerHelper.Object,
                val.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper);
            _postActionHttpTriggerService.Setup(x => x.CreateAsync(_action)).Returns(Task.FromResult<Models.Action>(_action));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _httpRequestHelper.Setup(x=>x.GetResourceFromRequest<Models.Action>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _resourceHelper.Setup(x=>x.DoesCustomerExist(_customerId)).Returns(Task.FromResult(false));
            _httpRequestHelper.Setup(x=>x.GetResourceFromRequest<Models.Action>(_request)).Returns(Task.FromResult(_action));
            var validationResults = new List<ValidationResult>();
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _postActionHttpTrigger = new PostActionHttpTrigger.Function.PostActionHttpTrigger(_resourceHelper.Object,
                _postActionHttpTriggerService.Object,
                _loggerHelper.Object,
                val.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateActionRecord()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Action>(_request)).Returns(Task.FromResult(_action));
            var validationResults = new List<ValidationResult>();
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _postActionHttpTrigger = new PostActionHttpTrigger.Function.PostActionHttpTrigger(_resourceHelper.Object,
                _postActionHttpTriggerService.Object,
                _loggerHelper.Object,
                val.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper);
            _postActionHttpTriggerService.Setup(x=>x.CreateAsync(_action)).Returns(Task.FromResult<Models.Action>(null));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns(_apimUrl);
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Action>(_request)).Returns(Task.FromResult(_action));
            var validationResults = new List<ValidationResult>();
            var val = new Mock<IValidate>();
            val.Setup(x => x.ValidateResource(It.IsAny<IAction>(), It.IsAny<bool>())).Returns(validationResults);
            _postActionHttpTrigger = new PostActionHttpTrigger.Function.PostActionHttpTrigger(_resourceHelper.Object,
                _postActionHttpTriggerService.Object,
                _loggerHelper.Object,
                val.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _guidHelper);
            _postActionHttpTriggerService.Setup(x => x.CreateAsync(_action)).Returns(Task.FromResult<Models.Action>(_action));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string sessionId, string actionplanId)
        {
            return await _postActionHttpTrigger.Run(
                _request,
                _log.Object,
                customerId,
                interactionId,
                actionplanId
               ).ConfigureAwait(false);
        }
    }
}