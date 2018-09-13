using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Configuration;
using Ocelot.OrleansHttpGateway.Requester;
using System.Threading.Tasks;

namespace Ocelot.OrleansHttpGateway
{
    public class OrleansRequesterMiddleware : OcelotMiddleware
    {
        private readonly OcelotRequestDelegate _next;
        private readonly OrleansRequesterConfiguration _config;
        private readonly IOrleansAuthorisation _authorisation;
        private readonly IClusterClientBuilder _clusterClientBuilder;
        private readonly IGrainReference _grainReference;
        private readonly IGrainMethodInvoker _grainInvoker;
        private readonly IRouteValuesBuilder _routeValuesBuilder;
        private readonly IOcelotLogger _logger;

        public OrleansRequesterMiddleware(OcelotRequestDelegate next,
            IClusterClientBuilder clusterClientBuilder,
            IGrainReference grainReference,
            IGrainMethodInvoker grainInvoker,
            IRouteValuesBuilder routeValuesBuilder,
            IOcelotLoggerFactory factory,
            IOptions<OrleansRequesterConfiguration> config,
            IOrleansAuthorisation authorisation,
            IOcelotLoggerFactory loggerFactory)
            : base(loggerFactory.CreateLogger<OrleansRequesterMiddleware>())
        {
            this._clusterClientBuilder = clusterClientBuilder;
            this._grainReference = grainReference;
            this._grainInvoker = grainInvoker;
            this._routeValuesBuilder = routeValuesBuilder;
            this._logger = factory.CreateLogger<OrleansRequesterMiddleware>();
            this._config = config?.Value;
            this._authorisation = authorisation;
            this._next = next;
        }

        public async Task Invoke(DownstreamContext context)
        {

            var routeResponse = this._routeValuesBuilder.Build(context);
            if (routeResponse.IsError)
            {
                Logger.LogDebug("IRouteValuesBuilder Parsing Route Values errors");
                SetPipelineError(context, routeResponse.Errors);
                return;
            }

            //Olreans interface authorization verification
            var authorised = this._authorisation.Authorise(context.HttpContext.User, routeResponse.Data);
            if (authorised.IsError)
            {
                _logger.LogWarning("error orleas authorising user scopes");
                SetPipelineError(context, authorised.Errors);
                return;
            }

            var clientBuilderResponse = _clusterClientBuilder.Build(routeResponse.Data, context);
            if (clientBuilderResponse.IsError)
            {
                Logger.LogDebug("IClusterClientBuilder Building Cluster Client and connecting Orleans error");
                SetPipelineError(context, routeResponse.Errors);
                return;
            }

            //Get a Grain instance
            var grainResponse = this._grainReference.GetGrainReference(routeResponse.Data);
            if (grainResponse.IsError)
            {
                Logger.LogDebug("IGrainReference Get a Orleas Grain instance error");
                SetPipelineError(context, routeResponse.Errors);
                return;
            }

            try
            {
                //Orleans injects the DownstreamContext into the RequestContext when requested
                this._config?.RequestContextInjection?.Invoke(context);
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Orleans injects the DownstreamContext into the RequestContext when requested error", ex);
                SetPipelineError(context, new UnknownError("Orleans injects the DownstreamContext into the RequestContext when requested error"));
                return;
            }

            //Grain Dynamic request

            var resultResponse = await _grainInvoker.Invoke(grainResponse.Data, routeResponse.Data);
            if (resultResponse.IsError)
            {
                Logger.LogDebug("IGrainMethodInvoker  Grain Dynamic request error");
                SetPipelineError(context, resultResponse.Errors);
                return;
            }
            Logger.LogDebug("setting http response message");
            context.HttpContext.Response.ContentType = resultResponse.Data.Content.ContentType;
            context.DownstreamResponse = new DownstreamResponse(resultResponse.Data.Content, resultResponse.Data.StatusCode, resultResponse.Data.Headers);

        }



    }
}