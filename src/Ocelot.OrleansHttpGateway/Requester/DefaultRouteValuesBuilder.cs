using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.DownstreamRouteFinder.Finder;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Configuration;
using Ocelot.OrleansHttpGateway.Infrastructure;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Request.Middleware;
using Ocelot.Responses;
using Orleans;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
    internal class DefaultRouteValuesBuilder : IRouteValuesBuilder
    {
        private readonly ConcurrentDictionary<string, Type> _GrainTypeCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, List<MethodInfo>> _MethodInfoCache = new ConcurrentDictionary<string, List<MethodInfo>>(StringComparer.OrdinalIgnoreCase);
        private readonly OrleansHttpGatewayOptions _options;
        private readonly OrleansRequesterConfiguration _config;
        private readonly IOcelotLogger _logger;

        public DefaultRouteValuesBuilder(IOptions<OrleansHttpGatewayOptions> options
            , IOptions<OrleansRequesterConfiguration> config
           , IOcelotLoggerFactory factory)
        {
            this._config = config?.Value;
            this._options = options.Value;
            this._logger = factory.CreateLogger<DefaultRouteValuesBuilder>();
        }
        public Response<GrainRouteValues> Build(DownstreamContext context)
        {
            try
            {
                GrainRouteValues route = this.GetRequestRoute(context);
                if (route == null)
                    return this.SetUnableToFindDownstreamRouteError(context, $"The request address is invalid URL:{context.DownstreamRequest.ToUri()}");

                this._logger.LogDebug($"Http address translation Orleans request address {context.DownstreamRequest.ToUri()}");

                //Get client option based on serviceName
                if (!_options.Clients.ContainsKey(route.SiloName))
                    return this.SetUnableToFindDownstreamRouteError(context, $"{nameof(OrleansClientOptions)} without {route.SiloName} configured ");
                route.ClientOptions = _options.Clients[route.SiloName];

                //Find the request corresponding to the Orleans interface
                route.GrainType = this.GetGrainType(route);
                if (route.GrainType == null)
                    return this.SetUnableToFindDownstreamRouteError(context, $"The request address is invalid,No corresponding Orleans interface  found. URL:{route.RequestUri}");
                //Find the request corresponding to the method in the Orleans interface
                route.GrainMethod = this.GetGtainMethods(route);
                if (route.GrainMethod == null)
                    return this.SetUnableToFindDownstreamRouteError(context, $"The request address is invalid and the corresponding method in the Orleans interface {route.SiloName}_#_{route.GrainName} was not found. {route.GrainMethodName}. URL:{route.RequestUri}");

                return new OkResponse<GrainRouteValues>(route);
            }
            catch (OrleansConfigurationException ex)
            {
                this._logger.LogError($"{nameof(OrleansConfigurationException)}   {ex.Message}", ex);
                return this.SetUnknownError(ex.Message);
            }
            catch (OrleansGrainReferenceException ex)
            {
                this._logger.LogError($"{nameof(OrleansGrainReferenceException)}   {ex.Message}", ex);
                return this.SetUnknownError(ex.Message);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message, ex);
                return this.SetUnknownError(ex.Message);
            }
        }

        private Type GetGrainType(GrainRouteValues route)
        {
            return _GrainTypeCache.GetOrAdd($"{route.SiloName}_#_{route.GrainName}", (key) =>
              {
                  this._logger.LogDebug($"Looking for the {route.ClientOptions.ServiceName} Grain interface");
                  if (this._config == null || this._config.MapRouteToGraininterface == null)
                      throw new OrleansConfigurationException($"{route.ClientOptions.ServiceName}Route map Grain interface configured to set");

                  try
                  {
                      string grainInterface = this._config.MapRouteToGraininterface.Invoke(route);
                      return route.ClientOptions.Assembly.ExportedTypes
                           .Where(f => typeof(IGrain).IsAssignableFrom(f) && f.Name.Equals(grainInterface, StringComparison.OrdinalIgnoreCase))
                           .FirstOrDefault();
                  }
                  catch (Exception ex)
                  {
                      throw new OrleansConfigurationException($"{route.ClientOptions.ServiceName} Orleans Client Options configuration error, unable to find Grain interface", ex);
                  }
              });
        }

        private MethodInfo GetGtainMethods(GrainRouteValues route)
        {
            var methods = _MethodInfoCache.GetOrAdd($"{route.SiloName}_#_{route.GrainName}_#_{route.GrainMethodName}", (key) =>
            {
                //Get grainType IEnumerable<MethodInfo> 
                return ReflectionUtil.GetMethodsIncludingBaseInterfaces(route.GrainType)
                        .Where(x => string.Equals(x.Name, route.GrainMethodName, StringComparison.OrdinalIgnoreCase)).ToList();
            });
            if (methods.Count == 0)
                return null;

            //Get the first method, temporarily only support one method with the same name
            return methods.FirstOrDefault();
        }

        private GrainRouteValues GetRequestRoute(DownstreamContext context)
        {
            GrainRouteValues routeValues = new GrainRouteValues();
            string[] route = context.DownstreamRequest.AbsolutePath.Split('/')
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
            if (route.Length < 2 || route.Length > 3)
                return null;

            routeValues.SiloName = context.DownstreamReRoute.ServiceName;
            routeValues.GrainName = route[0];
            routeValues.GrainMethodName = route[1];
            routeValues.RequestUri = context.DownstreamRequest.ToUri();

            //Claims to Claims Tranformation Whether to inject GrainKey
            var claim = context.HttpContext.User.FindFirst("GrainKey");
            if (claim != null)
                routeValues.GrainId = claim.Value;
            else
                routeValues.GrainId = (route.Length == 3) ? route[2] : null;

            try
            {
                routeValues.Querys = this.GetQueryParameter(context.DownstreamRequest.Query);
                routeValues.Body = this.GetBodyParameter(context.DownstreamRequest, context.HttpContext.Request);
            }
            catch (Exception ex)
            {
                throw new OrleansGrainReferenceException("Error parsing request parameters ...", ex);
            }
            return routeValues;
        }

        private IQueryCollection GetQueryParameter(string queryString)
        {
            var querys = Microsoft.AspNetCore.WebUtilities.QueryHelpers
                .ParseQuery(queryString);
            return new QueryCollection(querys);
        }

        private JObject GetBodyParameter(DownstreamRequest request, HttpRequest httpRequest)
        {
            var requestContentType = httpRequest.GetTypedHeaders().ContentType;
            if (requestContentType?.MediaType != "application/json")
                return new JObject();

            request.Scheme = "http";
            var requestMessage = request.ToHttpRequestMessage();
            request.Scheme = "orleans";
            if (requestMessage.Content == null)
                return new JObject();

            // parse encoding
            // default to UTF8
            var encoding = httpRequest.GetTypedHeaders().ContentType.Encoding ?? Encoding.UTF8;
            var stream = requestMessage.Content.ReadAsStreamAsync().Result;
            using (var reader = new JsonTextReader(new StreamReader(stream, encoding)))
            {
                reader.CloseInput = false;
                return JObject.Load(reader);
            }
        }

        private ErrorResponse<GrainRouteValues> SetUnknownError(string message)
        {
            return new ErrorResponse<GrainRouteValues>(new UnknownError(message));
        }


        private ErrorResponse<GrainRouteValues> SetUnableToFindDownstreamRouteError(DownstreamContext context, string message)
        {
            this._logger.LogWarning(message);
            return new ErrorResponse<GrainRouteValues>(new UnableToFindDownstreamRouteError(context.DownstreamRequest.ToUri(), context.DownstreamRequest.Scheme));
        }
    }


}
