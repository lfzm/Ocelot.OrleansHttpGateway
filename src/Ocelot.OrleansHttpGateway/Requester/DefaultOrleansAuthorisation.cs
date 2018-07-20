using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Ocelot.Authorisation;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Responses;
using IdentityModel;
using Ocelot.Infrastructure.Claims.Parser;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public class DefaultOrleansAuthorisation : IOrleansAuthorisation
    {

        public Response<bool> Authorise(ClaimsPrincipal claimsPrincipal, GrainRouteValues route)
        {
            var attrs = route.GrainMethod.GetCustomAttributes<AuthorizeAttribute>(true);
            if (attrs.Count() == 0)
                return new OkResponse<bool>(true);

            foreach (var attr in attrs)
            {
                if (!string.IsNullOrEmpty(attr.Roles))
                {
                    var response = this.AuthoriseRole(claimsPrincipal, attr.Roles);
                    if (response.IsError || !response.Data)
                        return response;
                }
                if (!string.IsNullOrEmpty(attr.Policy))
                {
                    var response = this.AuthorisePolicy(claimsPrincipal, attr.Policy);
                    if (response.IsError || !response.Data)
                        return response;
                }
            }
            return new OkResponse<bool>(true);
        }

        private Response<bool> AuthoriseRole(ClaimsPrincipal claimsPrincipal, string role)
        {
            var roles = role.Split(string.Empty.ToCharArray()).Where(f => !string.IsNullOrEmpty(f)).ToList();
         
            foreach (var r in roles)
            {
                if (claimsPrincipal.IsInRole(r))
                    return new OkResponse<bool>(true);
            }
            return new ErrorResponse<bool>(new ClaimValueNotAuthorisedError(
                        $"Authorization does not include role value {role}"));
        }

        private Response<bool> AuthorisePolicy(ClaimsPrincipal claimsPrincipal, string policy)
        {
            var policys = policy.Split(string.Empty.ToCharArray()).Where(f => !string.IsNullOrEmpty(f)).ToList();
            foreach (var p in policys)
            {
                IEnumerable<Claim> claims = claimsPrincipal.FindAll(p);
                if (claims.Count() == 0)
                {
                    return new ErrorResponse<bool>(new ClaimValueNotAuthorisedError(
                         $"Claim does not contain {p}"));
                }
            }
            return new OkResponse<bool>(true);
        }
    }
}
