using Ocelot.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public class UnknownError : Error
    {
        public UnknownError(string message) : base(message,  OcelotErrorCode.UnknownError)
        {
        }
    }
}
