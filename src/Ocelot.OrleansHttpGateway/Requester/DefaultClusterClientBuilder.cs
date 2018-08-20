using Microsoft.Extensions.Options;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Configuration;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Responses;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public class DefaultClusterClientBuilder : IClusterClientBuilder
    {
        private readonly ConcurrentDictionary<string, IClusterClient> clusterClientCache = new ConcurrentDictionary<string, IClusterClient>();
        private readonly OrleansHttpGatewayOptions _options;
        private readonly IOcelotLogger _logger;
        public DefaultClusterClientBuilder(IOptions<OrleansHttpGatewayOptions> options, IOcelotLoggerFactory factory)
        {
            this._logger = factory.CreateLogger<DefaultClusterClientBuilder>();
            this._options = options.Value;
        }
        public Response Build(GrainRouteValues routeValues, DownstreamContext context)
        {
            try
            {
                string clientKey = this.GetClientKey(routeValues.GrainType);
                if (clusterClientCache.Keys.Where(f => f == clientKey).Count() > 0)
                    return new OkResponse();

                clusterClientCache.GetOrAdd(clientKey, (key) =>
                {
                    return this.BuildClusterClient(routeValues, context);
                });
                return new OkResponse();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message, ex);
                return new ErrorResponse<GrainRouteValues>(new UnknownError(ex.Message));
            }
        }

        public IClusterClient GetClusterClient<TGrainInterface>()
        {
            string cacheKey = this.GetClientKey(typeof(TGrainInterface));
            if (!clusterClientCache.TryGetValue(cacheKey, out IClusterClient clusterClient))
                throw new Exception("Get IClusterClient does not exist");
            return clusterClient;
        }

        private IClusterClient BuildClusterClient(GrainRouteValues routeValues, DownstreamContext context)
        {
            var clientOptions = routeValues.ClientOptions;

            IClientBuilder build = new ClientBuilder();
            build.Configure<ClusterOptions>(opt =>
            {
                opt.ClusterId = clientOptions.ClusterId;
                opt.ServiceId = clientOptions.ServiceId;
            });

            build = this.UseServiceDiscovery(build, context);
            var client = build.Build();

            return this.ConnectClient(routeValues.SiloName, client);

        }

        private IClientBuilder UseServiceDiscovery(IClientBuilder build, DownstreamContext context)
        {
            if (context.DownstreamReRoute.DownstreamAddresses.Count > 0)
            {
                List<IPEndPoint> endpoints = new List<IPEndPoint>();
                foreach (var address in context.DownstreamReRoute.DownstreamAddresses)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address.Host), address.Port);
                    endpoints.Add(endPoint);
                }
                this._logger.LogDebug($"Orleans uses Static Clustering's ");
                build.UseStaticClustering(endpoints.ToArray());
                return build;
            }

            //TODO：Determine if it is Consul load balancing
            if (context.Configuration.ServiceProviderConfiguration != null)
            {
                var config = context.Configuration.ServiceProviderConfiguration;
                if (config.Type.Equals("consul", StringComparison.OrdinalIgnoreCase))
                {
                    build.UseConsulClustering(opt =>
                    {
                        opt.Address = new Uri($"http://{config.Host}:{config.Port}");
                        this._logger.LogDebug($"Orleans uses Consul Clustering's address is { opt.Address.ToString()}");
                    });
                    return build;
                }
                this._logger.LogError($"Does not support {config.Type} service discovery", null);
                throw new OrleansConfigurationException($"Does not support {config.Type} service discovery");
            }

            throw new OrleansConfigurationException($"No service discovery address configured");

        }
        private IClusterClient ConnectClient(string serviceName, IClusterClient client)
        {
            try
            {
                client.Connect(RetryFilter).Wait();
                _logger.LogDebug($"Connection {serviceName} Sucess...");
                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Connection {serviceName} Faile...", ex);
                throw new OrleansConnectionFailedException($"Connection {serviceName} Faile...");
            }
        }
        private int attempt = 0;
        private async Task<bool> RetryFilter(Exception exception)
        {
            if (exception.GetType() != typeof(SiloUnavailableException))
            {
                Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
                return false;
            }
            attempt++;
            Console.WriteLine($"Cluster client attempt {attempt} of {this._options.InitializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
            if (attempt > this._options.InitializeAttemptsBeforeFailing)
            {
                return false;
            }
            await Task.Delay(TimeSpan.FromSeconds(4));
            return true;
        }
        private string GetClientKey(Type type)
        {
            return type.Assembly.Location;
        }
    }
}
