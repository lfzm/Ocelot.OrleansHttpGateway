using Ocelot.Middleware;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Configuration
{
    public class OrleansRequesterConfiguration
    {
        public Action<DownstreamContext> RequestContextInjection { get; set; }
    }
}
