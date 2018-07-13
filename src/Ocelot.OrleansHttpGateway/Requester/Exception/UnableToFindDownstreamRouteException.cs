using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
  public  class UnableToFindDownstreamRouteException : Exception
    {

        /// <summary>
        /// Creates a new <see cref="UnableToFindDownstreamRouteException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public UnableToFindDownstreamRouteException(string message)
            : base(message)
        {

        }

    
    }
}
