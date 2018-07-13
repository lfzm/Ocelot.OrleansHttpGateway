using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocelot.OrleansHttpGateway.Model;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public interface IGrainMethodInvoker
    {
        Task<object> Invoke(Model.GrainReference grain, GrainRouteValues route);
    }


}