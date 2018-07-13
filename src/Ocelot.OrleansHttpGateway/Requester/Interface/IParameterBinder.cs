using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocelot.OrleansHttpGateway.Model;

namespace Ocelot.OrleansHttpGateway.Requester
{
    /// <summary>
    /// Parse HttpContext to get parameters and bind to Grain Methods
    /// </summary>
    public interface IParameterBinder
    {
        object[] BindParameters(ParameterInfo[] parameters, GrainRouteValues routeValues);
    }
}