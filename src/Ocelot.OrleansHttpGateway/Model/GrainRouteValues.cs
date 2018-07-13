using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Configuration;
using Ocelot.OrleansHttpGateway.Requester;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Model
{
    public class GrainRouteValues
    {
        public GrainRouteValues()
        {
          
        }
        public OrleansClientOptions ClientOptions { get; set; }
        public Type GrainType { get; set; }
        public string SiloName { get; set; }
        public string GrainName { get; set; }
        public string GrainMethod { get; set; }
        public string GrainId { get; set; }
        public IQueryCollection Querys { get; set; }
        public JObject Body { get; set; }

      
    }
}
