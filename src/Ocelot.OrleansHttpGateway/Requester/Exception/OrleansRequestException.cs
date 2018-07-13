using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
   public class OrleansRequestException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="OrleansRequestException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public OrleansRequestException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="OrleansRequestException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public OrleansRequestException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
