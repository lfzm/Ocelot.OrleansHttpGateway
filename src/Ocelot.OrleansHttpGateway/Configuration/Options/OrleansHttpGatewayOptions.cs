using System.Collections.Generic;

namespace Ocelot.OrleansHttpGateway.Configuration
{
    public class OrleansHttpGatewayOptions
    {

        public Dictionary<string,OrleansClientOptions> Clients { get; set; } = new Dictionary<string, OrleansClientOptions>();
      
        public  int InitializeAttemptsBeforeFailing { get; set; } = 10;


    }

  
}