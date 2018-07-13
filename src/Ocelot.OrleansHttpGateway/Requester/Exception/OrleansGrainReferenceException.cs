using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
   public class OrleansGrainReferenceException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="OrleansGrainReferenceException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public OrleansGrainReferenceException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="OrleansGrainReferenceException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public OrleansGrainReferenceException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
