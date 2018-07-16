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
        private readonly IClaimsParser _claimsParser;
        public DefaultOrleansAuthorisation(IClaimsParser claimsParser)
        {
            this._claimsParser = claimsParser;
        }

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
                    var response = this.AuthorisePolicy(claimsPrincipal, attr.Roles);
                    if (response.IsError || !response.Data)
                        return response;
                }
            }
            return new OkResponse<bool>(true);
        }

        private Response<bool> AuthoriseRole(ClaimsPrincipal claimsPrincipal, string role)
        {
            var roles = role.Split(string.Empty.ToCharArray()).Where(f => !string.IsNullOrEmpty(f)).ToList();
            var values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, JwtClaimTypes.Role);
            if (values.IsError)
                return new ErrorResponse<bool>(values.Errors);


            if (values.Data == null)
                return new ErrorResponse<bool>(new UserDoesNotHaveClaimError($"user does not have claim {JwtClaimTypes.Role}"));

            bool authorised = roles.Intersect(values.Data).ToArray().Length > 0;
            if (!authorised)
            {
                return new ErrorResponse<bool>(new ClaimValueNotAuthorisedError(
                        $"claim value: {values.Data} is not the same as required value:{role} for type:  { JwtClaimTypes.Role}"));
            }
            else
                return new OkResponse<bool>(true);
        }

        private Response<bool> AuthorisePolicy(ClaimsPrincipal claimsPrincipal, string policy)
        {
            var policys = policy.Split(string.Empty.ToCharArray()).Where(f => !string.IsNullOrEmpty(f)).ToList();
            foreach (var p in policys)
            {
                var values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, p);
                if (values.IsError)
                    return new ErrorResponse<bool>(values.Errors);

                if (values.Data.Count == 0)
                {
                    return new ErrorResponse<bool>(new ClaimValueNotAuthorisedError(
                         $"Claim does not contain {p}"));
                }
            }
            return new OkResponse<bool>(true);
        }
    }
}
