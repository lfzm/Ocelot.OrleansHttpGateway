using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public class OrleansConnectionFailedException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="OrleansConnectionFailedException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public OrleansConnectionFailedException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="OrleansConnectionFailedException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public OrleansConnectionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
