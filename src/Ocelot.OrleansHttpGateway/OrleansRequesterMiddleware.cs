using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Requester;
using System.Threading.Tasks;

namespace Ocelot.OrleansHttpGateway
{
    public class OrleansRequesterMiddleware : OcelotMiddleware
    {
        private readonly OcelotRequestDelegate _next;
        private readonly IOrleansRequester _requester;

        public OrleansRequesterMiddleware(OcelotRequestDelegate next,
            IOrleansRequester requester,
            IOcelotLoggerFactory loggerFactory)
            : base(loggerFactory.CreateLogger<OrleansRequesterMiddleware>())
        {
            _requester = requester;
            _next = next;
        }

        public async Task Invoke(DownstreamContext context)
        {
            var response = await _requester.GetResponse(context);
            if (response.IsError)
            {
                Logger.LogDebug("IOrleansRequester returned an error, setting pipeline error");

                SetPipelineError(context, response.Errors);
                return;
            }
            Logger.LogDebug("setting http response message");
            context.HttpContext.Response.ContentType = "application/json";
            context.DownstreamResponse = new DownstreamResponse(response.Data.Content, response.Data.StatusCode, response.Data.Headers);
        }


      
    }
}