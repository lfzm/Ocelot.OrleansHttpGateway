using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.Concurrency;
using Ocelot.Orleans.Configuration;

namespace Ocelot.Orleans.Core
{
    public class JsonBodyParameterBinder : IParameterBinder
    {
        private readonly JsonSerializer _serializer;

        public string Name => "JsonBody_ParameterBinder";

        public JsonBodyParameterBinder(JsonSerializer serializer)
        {
            _serializer = serializer;
        }


        public Task<bool> CanBind(ParameterInfo[] parameters, HttpRequest request)
        {
            //some form of content negotiation here...

            //parse mediatype
            var requestContentType = request.GetTypedHeaders().ContentType;
            if (requestContentType?.MediaType == "application/json" && parameters.Length > 0)
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public async Task<object[]> BindParameters(ParameterInfo[] parameters, HttpRequest request)
        {
            var result = new object[parameters.Length];

            if (request.ContentLength == 0)
            {
                return result;
            }

            if (!request.Body.CanSeek)
            {
                // JSON.Net does synchronous reads. In order to avoid blocking on the stream, we asynchronously 
                // read everything into a buffer, and then seek back to the beginning. 
                request.EnableRewind();
            }
            request.Body.Seek(0L, SeekOrigin.Begin);

            // parse encoding
            // default to UTF8
            var encoding = request.GetTypedHeaders().ContentType.Encoding ?? Encoding.UTF8;

            using (var reader = new JsonTextReader(new StreamReader(request.Body, encoding)))
            {
                reader.CloseInput = false;
                var root = await JObject.LoadAsync(reader);

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    if (root.TryGetValue(parameter.Name, StringComparison.OrdinalIgnoreCase, out JToken value))
                    {
                        result[i] = value.ToObject(parameter.ParameterType, _serializer);
                    }
                }
            }
            return result;
        }
    }
}