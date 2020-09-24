using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.PostActionHttpTrigger.Service;
using NCS.DSS.Action.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace NCS.DSS.Action.Tests.FunctionTests
{
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

        private readonly ILogger _log;
        private readonly HttpRequest _request;
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IPostActionHttpTriggerService _postActionHttpTriggerService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IGuidHelper _guidHelper;
        private readonly Models.Action _action;
        private readonly PostActionHttpTrigger.Function.PostActionHttpTrigger _postActionHttpTrigger;

        public PostActionHttpTriggerTests()
        {
            _action = Substitute.For<Models.Action>();
            _request = new DefaultHttpRequest(new DefaultHttpContext());
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();

            var jsonHelper = Substitute.For<IJsonHelper>();
            var loggerHelper = Substitute.For<ILoggerHelper>();
            _guidHelper = Substitute.For<IGuidHelper>();

            _log = Substitute.For<ILogger>(); _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _postActionHttpTriggerService = Substitute.For<IPostActionHttpTriggerService>();

            _postActionHttpTrigger = Substitute.For<PostActionHttpTrigger.Function.PostActionHttpTrigger>(_resourceHelper,
                _postActionHttpTriggerService,
                loggerHelper,
                _validate,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                jsonHelper,
                _guidHelper);
            
            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:");
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(_interactionId, _customerId).Returns(true);
            _resourceHelper.DoesActionPlanExistAndBelongToCustomer(_actionPlanId, _interactionId, _customerId).Returns(true);
            _httpRequestHelper.GetResourceFromRequest<Models.Action>(_request).Returns(Task.FromResult(_action).Result);
            _resourceHelper.DoesCustomerExist(_customerId).ReturnsForAnyArgs(true);
            _guidHelper.ValidateGuid(ValidCustomerId).Returns(_customerId);
            _guidHelper.ValidateGuid(ValidInteractionId).Returns(_interactionId);
            _guidHelper.ValidateGuid(ValidActionPlanId).Returns(_actionPlanId);

            SetUpHttpResponseMessageHelper();
        }

        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            _httpRequestHelper.GetDssApimUrl(_request).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionHasFailedValidation()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Action>(_request).Returns(Task.FromResult(_action).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(_action, true).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
        }

        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.Action>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal((HttpStatusCode)422, result.StatusCode);
        }

        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(_customerId).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }
        
        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateActionRecord()
        {
            _postActionHttpTriggerService.CreateAsync(_action).Returns(Task.FromResult<Models.Action>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
        
        [Fact]
        public async Task PostActionHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _postActionHttpTriggerService.CreateAsync(_action).Returns(Task.FromResult(_action).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string sessionId, string actionplanId)
        {
            return await _postActionHttpTrigger.Run(
                _request,
                _log,
                customerId,
                interactionId,
                actionplanId
               ).ConfigureAwait(false);
        }

        private void SetUpHttpResponseMessageHelper()
        {
            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>())
                .Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<HttpRequest>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<JsonException>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            _httpResponseMessageHelper.Forbidden().Returns(x => new HttpResponseMessage(HttpStatusCode.Forbidden));

            _httpResponseMessageHelper.Conflict().Returns(x => new HttpResponseMessage(HttpStatusCode.Conflict));

            _httpResponseMessageHelper
                .Created(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.Created));

        }
    }
}