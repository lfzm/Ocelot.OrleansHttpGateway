using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Responses;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public interface IOrleansAuthorisation
    {
        Response<bool> Authorise(ClaimsPrincipal claimsPrincipal, GrainRouteValues routeValues);
    }
}
