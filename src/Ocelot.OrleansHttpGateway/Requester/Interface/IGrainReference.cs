using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Responses;
using System;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public interface IGrainReference
    {
        Response<GrainReference> GetGrainReference(GrainRouteValues grain);
    }


}