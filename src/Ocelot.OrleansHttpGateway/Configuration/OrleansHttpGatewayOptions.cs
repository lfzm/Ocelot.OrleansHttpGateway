using Ocelot.Middleware;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ocelot.OrleansHttpGateway.Configuration
{
    public class OrleansHttpGatewayOptions
    {
        public Dictionary<string, OrleansClientOptions> Clients { get; set; } = new Dictionary<string, OrleansClientOptions>();

        public int InitializeAttemptsBeforeFailing { get; set; } = 10;

        /// <summary>
        /// add client
        /// </summary>
        /// <param name="serviceName">serviceName</param>
        /// <param name="serviceId">serviceId</param>
        /// <param name="clusterId">clusterId</param>
        /// <param name="assembly">clusterId</param>
        public void AddClient(string serviceName,string serviceId,string clusterId, Assembly assembly)
        {
            Clients.Add(serviceName, new OrleansClientOptions
            {
                Assembly = assembly,
                ClusterId = clusterId,
                ServiceName=serviceName,
                ServiceId=serviceId
            });
        }
    }


}