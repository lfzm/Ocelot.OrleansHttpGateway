using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public class OrleansConfigurationException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="OrleansConfigurationException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public OrleansConfigurationException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="OrleansConfigurationException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public OrleansConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
