using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Ocelot.DownstreamRouteFinder.Finder;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Configuration;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.OrleansHttpGateway.Requester;
using Ocelot.Request.Middleware;
using Ocelot.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.OrleansHttpGateway.Test
{
    public class OrleansRequesterMiddlewareTests
    {
        private readonly DownstreamContext context;
        private readonly OcelotRequestDelegate _next;
        private readonly Mock<IOptions<OrleansRequesterConfiguration>> _config;
        private readonly Mock<IOrleansAuthorisation> _authorisation;
        private readonly Mock<IClusterClientBuilder> _clusterClientBuilder;
        private readonly Mock<IGrainReference> _grainReference;
        private readonly Mock<IGrainMethodInvoker> _grainInvoker;
        private readonly Mock<IRouteValuesBuilder> _routeValuesBuilder;
        private readonly Mock<IOcelotLogger> _logger;
        private readonly Mock<IOcelotLoggerFactory> _loggerFactory;
        private readonly OrleansRequesterMiddleware _middleware;

        public OrleansRequesterMiddlewareTests()
        {
            context = new DownstreamContext(new DefaultHttpContext());
            context.DownstreamRequest = new DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "http://test.com"));
            _next = context =>
            {
                return Task.CompletedTask;
            };
            _config = new Mock<IOptions<OrleansRequesterConfiguration>>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<OrleansRequesterMiddleware>()).Returns(_logger.Object);
            _authorisation = new Mock<IOrleansAuthorisation>();
            _clusterClientBuilder = new Mock<IClusterClientBuilder>();
            _grainReference = new Mock<IGrainReference>();
            _grainInvoker = new Mock<IGrainMethodInvoker>();
            _routeValuesBuilder = new Mock<IRouteValuesBuilder>();
            _middleware = new OrleansRequesterMiddleware(_next, _clusterClientBuilder.Object, _grainReference.Object, _grainInvoker.Object, _routeValuesBuilder.Object, _config.Object, _authorisation.Object, _loggerFactory.Object);
        }
       [Fact]
        public void should_request_success()
        {
            this.Given(x => x.GivenHeaderSuccess())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenMiddlewareHeaderSuccess());
        }

        [Fact]
        public void should_request_fail()
        {
            this.Given(x => x.GivenHeaderSuccess())
                .Given(x =>x.GivenRouteValuesBuilderError())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenMiddlewareHeaderSuccess());
        }

        private void GivenHeaderSuccess()
        {
            var route = new GrainRouteValues();
            _routeValuesBuilder.Setup(x => x.Build(this.context)).Returns(new OkResponse<GrainRouteValues>(route));
            _authorisation.Setup(x => x.Authorise(this.context.HttpContext.User, route)).Returns(new OkResponse<bool>(true));
            var grain = new GrainReference(null, null);
            _clusterClientBuilder.Setup(x => x.Build(route, this.context)).Returns(new OkResponse<GrainReference>(grain));
            Response<OrleansResponseMessage> response = new OkResponse<OrleansResponseMessage>(new OrleansResponseMessage(null, HttpStatusCode.OK));
            _grainInvoker.Setup(x => x.Invoke(grain, route)).Returns(Task.FromResult(response));
        }


        private void GivenRouteValuesBuilderError()
        {
            var result = new ErrorResponse<GrainRouteValues>(new UnableToFindDownstreamRouteError(context.DownstreamRequest.ToUri(), context.DownstreamRequest.Scheme));
            _routeValuesBuilder.Setup(x => x.Build(this.context)).Returns(result);

        }
        private void WhenICallTheMiddleware()
        {
            this._middleware.Invoke(context).GetAwaiter().GetResult();
        }

        private void ThenMiddlewareHeaderSuccess()
        {
            Assert.NotNull(this.context.DownstreamResponse);
            Assert.Equal(HttpStatusCode.OK, this.context.DownstreamResponse.StatusCode);
        }

        private void ThenMiddlewareHeaderFail()
        {
            Assert.False(context.IsError);
        }
    }
}
