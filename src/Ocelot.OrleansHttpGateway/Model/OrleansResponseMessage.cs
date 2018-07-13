using Ocelot.Middleware;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Model
{
    public class OrleansResponseMessage
    {
        public OrleansResponseMessage(OrleansContent content, HttpStatusCode statusCode, List<Header> headers = null)
        {
            Content = content;
            StatusCode = statusCode;
            Headers = headers ?? new List<Header>();
        }

        public OrleansContent Content { get; }
        public HttpStatusCode StatusCode { get; }
        public List<Header> Headers { get; }
    }
}
