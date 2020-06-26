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
using NCS.DSS.Action.Cosmos.Helper;
using NCS.DSS.Action.GetActionHttpTrigger.Service;
using NSubstitute;
using Xunit;

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

        private readonly ILogger _log;
        private readonly HttpRequest _request;
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetActionHttpTriggerService _getActionHttpTriggerService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IGuidHelper _guidHelper;
        private readonly GetActionHttpTrigger.Function.GetActionHttpTrigger _getActionHttpTrigger;


        public GetActionHttpTriggerTest()
        {
            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();

            var loggerHelper = Substitute.For<ILoggerHelper>();
            var jsonHelper = Substitute.For<IJsonHelper>();
            _guidHelper = Substitute.For<IGuidHelper>();

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _getActionHttpTriggerService = Substitute.For<IGetActionHttpTriggerService>();

            _getActionHttpTrigger = Substitute.For<GetActionHttpTrigger.Function.GetActionHttpTrigger>(
                _resourceHelper,
                _getActionHttpTriggerService,
                loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                jsonHelper,
                _guidHelper);

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _resourceHelper.DoesCustomerExist(_customerId).Returns(true);
            _guidHelper.ValidateGuid(ValidCustomerId).Returns(_customerId);
            _guidHelper.ValidateGuid(ValidInteractionId).Returns(_interactionId);
            _guidHelper.ValidateGuid(ValidActionPlanId).Returns(_actionPlanId);

            _resourceHelper.DoesInteractionExistAndBelongToCustomer(_interactionId, _customerId).Returns(true);
            _resourceHelper.DoesActionPlanExistAndBelongToCustomer(_actionPlanId, _interactionId, _customerId).Returns(true);
            SetUpHttpResponseMessageHelper();
        }

        [Fact]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenDssCorrelationIdIsInvalid()
        {
            _httpRequestHelper.GetDssCorrelationId(_request).Returns(InValidId);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(_customerId).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(_interactionId, _customerId).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenActionDoesNotExist()
        {
            _getActionHttpTriggerService.GetActionsAsync(_customerId, _actionPlanId).Returns(Task.FromResult<List<Models.Action>>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeOk_WhenActionExists()
        {
            var listOfActions = new List<Models.Action>();
            _getActionHttpTriggerService.GetActionsAsync(_customerId, _actionPlanId).Returns(Task.FromResult(listOfActions).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsType<HttpResponseMessage>(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await _getActionHttpTrigger.Run(
                _request,
                _log,
                customerId,
                interactionId,
                actionPlanId).ConfigureAwait(false);
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
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

        }
    }
}