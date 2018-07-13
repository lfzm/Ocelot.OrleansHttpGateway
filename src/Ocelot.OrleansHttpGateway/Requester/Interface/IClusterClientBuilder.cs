using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Model;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public interface IClusterClientBuilder
    {
        IClusterClient GetClusterClient<TGrainInterface>();
        void Build(GrainRouteValues routeValues, DownstreamContext context);
    }
}
