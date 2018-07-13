using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Requester;
using Ocelot.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public interface IOrleansRequester
    {
        Task<Response<OrleansResponseMessage>> GetResponse(DownstreamContext context);
    }
}
