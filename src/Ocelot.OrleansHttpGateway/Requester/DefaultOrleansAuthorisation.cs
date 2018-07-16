using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Ocelot.Authorisation;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Responses;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public class DefaultOrleansAuthorisation : IOrleansAuthorisation
    {
        private readonly IClaimsAuthoriser _claimsAuthoriser;

        public Response<bool> Authorise(ClaimsPrincipal claimsPrincipal, GrainRouteValues route)
        {
            
            throw new NotImplementedException();
        }
    }
}
