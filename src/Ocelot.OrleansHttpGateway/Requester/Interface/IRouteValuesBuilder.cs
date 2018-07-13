using Microsoft.AspNetCore.Http;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
    /// <summary>
    /// Resolve RouteData Get <see cref="GrainRouteValues"/>
    /// </summary>
    public interface IRouteValuesBuilder
    {
        GrainRouteValues Build(DownstreamContext context);
    }
}
