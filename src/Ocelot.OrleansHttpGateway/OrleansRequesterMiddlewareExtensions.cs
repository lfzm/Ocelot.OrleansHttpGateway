using Ocelot.Middleware.Pipeline;
using Ocelot.OrleansHttpGateway;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.Responder.Middleware
{
    public static class OrleansRequesterMiddlewareExtensions
    {
        public static IOcelotPipelineBuilder UseOrleansRequesterMiddleware(this IOcelotPipelineBuilder builder)
        {
            return builder.UseMiddleware<OrleansRequesterMiddleware>();
        }
    }
}
