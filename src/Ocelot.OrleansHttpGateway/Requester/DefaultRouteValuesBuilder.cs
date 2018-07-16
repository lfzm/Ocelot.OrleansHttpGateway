using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Configuration;
using Ocelot.OrleansHttpGateway.Model;
using Orleans;

namespace Ocelot.OrleansHttpGateway.Requester
{
    internal class DefaultRouteValuesBuilder : IRouteValuesBuilder
    {
        private readonly ConcurrentDictionary<string, Type> _GrainTypeCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly OrleansHttpGatewayOptions _options;
        private readonly IClaimsParser _claimsParser;
        private readonly IOcelotLogger _logger;

        public DefaultRouteValuesBuilder(IOptions<OrleansHttpGatewayOptions> config, IOcelotLoggerFactory factory, IClaimsParser claimsParser)
        {
            this._options = config.Value;
            this._logger = factory.CreateLogger<DefaultRouteValuesBuilder>();
            this._claimsParser = claimsParser;
        }
        public GrainRouteValues Build(DownstreamContext context)
        {
            GrainRouteValues route = this.ResolveRequestContext(context);
            this._logger.LogDebug($"Http address translation Orleans request address {context.DownstreamRequest.ToUri()}");

            //Get client option based on serviceName
            if (!_options.Clients.ContainsKey(route.SiloName))
                throw new OrleansConfigurationException($"{nameof(OrleansClientOptions)} without {route.SiloName} configured ");
            route.ClientOptions = _options.Clients[route.SiloName];

            route.GrainType = _GrainTypeCache.GetOrAdd($"{route.SiloName}_#_{route.GrainName}", (key) => this.GetGrainInterface(route.ClientOptions, route.SiloName, route.GrainName));
            if (route.GrainType == null)
                throw new UnableToFindDownstreamRouteException($"The request address is invalid,No corresponding Orleans interface found. URL:{context.DownstreamRequest.ToUri()}");

            return route;
        }

        private Type GetGrainInterface(OrleansClientOptions clientOption, string siloName, string grainName)
        {
            this._logger.LogDebug($"Looking for the {siloName} Grain interface");
            if (string.IsNullOrEmpty(clientOption.InterfaceNameTemplate))
            {
                throw new OrleansConnectionFailedException($"{siloName} Interface Name Template not set");
            }
            try
            {
                string grainInterface = clientOption.InterfaceNameTemplate.Replace("{GrainName}", grainName);
                Type _type = clientOption.Assembly.ExportedTypes
                   .Where(f => typeof(IGrain).IsAssignableFrom(f) && f.Name.Equals(grainInterface, StringComparison.OrdinalIgnoreCase))
                   .FirstOrDefault();
                return _type;
            }
            catch (Exception ex)
            {
                throw new OrleansConfigurationException($"{siloName} Orleans Client Options configuration error, unable to find Grain interface", ex);
            }
        }

        private GrainRouteValues ResolveRequestContext(DownstreamContext context)
        {
            GrainRouteValues routeValues = new GrainRouteValues();
            string[] route = context.DownstreamRequest.AbsolutePath.Split('/')
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
            if (route.Length < 2 || route.Length > 3)
            {
                throw new UnableToFindDownstreamRouteException($"The request address is invalid URL:{context.DownstreamRequest.ToUri()}");
            }
            routeValues.SiloName = context.DownstreamReRoute.ServiceName;
            routeValues.GrainName = route[0];
            routeValues.GrainMethod = route[1];

            //Claims to Claims Tranformation Whether to inject GrainKay
            var response = this._claimsParser.GetValue(context.HttpContext.User.Claims, "GrainKay", "", 0);
            if (!response.IsError)
                routeValues.GrainId = response.Data;
            else
                routeValues.GrainId = (route.Length == 3) ? route[2] : null;

            try
            {
                routeValues.Querys = this.ResolveQuery(context.DownstreamRequest.Query);
                routeValues.Body = this.ResolveBody(context.HttpContext.Request);
            }
            catch (Exception ex)
            {
                throw new OrleansGrainReferenceException("Error parsing request parameters ...", ex);
            }
            return routeValues;
        }

        private IQueryCollection ResolveQuery(string queryString)
        {
            var querys = Microsoft.AspNetCore.WebUtilities.QueryHelpers
                .ParseQuery(queryString);
            return new QueryCollection(querys);
        }

        private JObject ResolveBody(HttpRequest request)
        {
            var requestContentType = request.GetTypedHeaders().ContentType;
            if (requestContentType?.MediaType != "application/json")
                return new JObject();

            if (!request.Body.CanSeek)
            {
                // JSON.Net does synchronous reads. In order to avoid blocking on the stream, we asynchronously 
                // read everything into a buffer, and then seek back to the beginning. 
                request.EnableRewind();
            }
            request.Body.Seek(0L, SeekOrigin.Begin);

            // parse encoding
            // default to UTF8
            var encoding = request.GetTypedHeaders().ContentType.Encoding ?? Encoding.UTF8;
            if (request.Body.Length == 0)
                return new JObject();
            using (var reader = new JsonTextReader(new StreamReader(request.Body, encoding)))
            {
                reader.CloseInput = false;
                return JObject.Load(reader);
            }
        }

    }


}
