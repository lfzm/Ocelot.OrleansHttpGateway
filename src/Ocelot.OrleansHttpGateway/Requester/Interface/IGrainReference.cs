using Ocelot.OrleansHttpGateway.Model;
using System;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public interface IGrainReference
    {
        GrainReference GetGrainReference(GrainRouteValues grain);
    }


}