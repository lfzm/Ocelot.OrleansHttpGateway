using Newtonsoft.Json;
using Ocelot.OrleansHttpGateway.Infrastructure;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Ocelot.OrleansHttpGateway.Model
{
    public class OrleansContent : HttpContent
    {
        private object _result;
        private JsonSerializer _jsonSerializer;

        public string ContentType = "application/json";
        public OrleansContent(object result, JsonSerializer jsonSerializer)
        {
            this.Headers.ContentLength = null;
            this._result = result;
            this._jsonSerializer = jsonSerializer;
            if (!_result.GetType().CanHaveChildren())
            {
                ContentType = "application/text";
            }
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            //StreamWriter writer = new StreamWriter(stream);
            //writer.Write(JsonConvert.SerializeObject(_result));
            //await writer.FlushAsync();

            var writer = new StreamWriter(stream);
            if (ContentType.Equals("application/json"))
            {
                this._jsonSerializer.Serialize(writer, _result);
                await writer.FlushAsync();
            }
            else
            {
                writer.WriteLine(_result.ToString());
                await writer.FlushAsync();
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return true;
        }
    }


}
