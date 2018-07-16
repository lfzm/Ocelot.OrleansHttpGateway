using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Responses;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public interface IGrainMethodInvoker
    {
        Task<Response<OrleansResponseMessage>> Invoke(GrainReference grain, GrainRouteValues route);
    }


}