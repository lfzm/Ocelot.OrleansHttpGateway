using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public class OrleansRequestTimedOutException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="OrleansRequestTimedOutException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public OrleansRequestTimedOutException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="OrleansRequestTimedOutException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public OrleansRequestTimedOutException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
