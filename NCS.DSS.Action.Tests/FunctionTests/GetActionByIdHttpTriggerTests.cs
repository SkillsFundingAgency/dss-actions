using System;
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
using NCS.DSS.Action.GetActionByIdHttpTrigger.Service;
using NSubstitute;
using Xunit;

namespace NCS.DSS.Action.Tests.FunctionTests
{
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
        
        private readonly ILogger _log;
        private readonly HttpRequest _request;
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetActionByIdHttpTriggerService _getActionByIdHttpTriggerService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly Models.Action _action;
        private readonly IGuidHelper _guidHelper;
        private readonly GetActionByIdHttpTrigger.Function.GetActionByIdHttpTrigger _getActionByIdHttpTrigger;

        public GetActionByIdHttpTriggerTests()
        {
            _action = Substitute.For<Models.Action>();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();

            var loggerHelper = Substitute.For<ILoggerHelper>();
            var jsonHelper = Substitute.For<IJsonHelper>();
            _guidHelper = Substitute.For<IGuidHelper>();

            _getActionByIdHttpTriggerService = Substitute.For<IGetActionByIdHttpTriggerService>();

            _getActionByIdHttpTrigger = Substitute.For<GetActionByIdHttpTrigger.Function.GetActionByIdHttpTrigger>(
                _resourceHelper,
                _getActionByIdHttpTriggerService,
                loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                jsonHelper,
                _guidHelper
                );

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _resourceHelper.DoesActionPlanExistAndBelongToCustomer(_actionPlanId, _interactionId, _customerId).Returns(true);

            _resourceHelper.DoesCustomerExist(_customerId).Returns(true);
            _guidHelper.ValidateGuid(ValidCustomerId).Returns(_customerId);
            _guidHelper.ValidateGuid(ValidInteractionId).Returns(_interactionId);
            _guidHelper.ValidateGuid(ValidActionPlanId).Returns(_actionPlanId);
            _guidHelper.ValidateGuid(ValidActionId).Returns(_actionId);
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(_interactionId, _customerId).Returns(true);
            _getActionByIdHttpTriggerService.GetActionForCustomerAsync(_customerId, _actionId, _actionPlanId).Returns(Task.FromResult(_action).Result);

            SetUpHttpResponseMessageHelper();
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenDssCorrelationIdIsInvalid()
        {
            _httpRequestHelper.GetDssCorrelationId(_request).Returns(InValidId);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId,  ValidActionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(_customerId).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(_interactionId, _customerId).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenActionDoesNotExist()
        {
            _getActionByIdHttpTriggerService.GetActionForCustomerAsync(_customerId, _actionId, _actionPlanId).Returns(Task.FromResult<Models.Action>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetActionsByIdHttpTrigger_ReturnsStatusCodeOk_WhenActionExists()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionId, ValidActionPlanId);
       
            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionId, string actionPlanId)
        {
            return await _getActionByIdHttpTrigger.Run(
                _request,
                _log,
                customerId,
                interactionId,
                actionPlanId,
                actionId).ConfigureAwait(false);
        }

        private void SetUpHttpResponseMessageHelper()
        {
            _httpResponseMessageHelper.BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            _httpResponseMessageHelper
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

        }
    }
}