using Newtonsoft.Json;
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
        public OrleansContent(object result, JsonSerializer jsonSerializer)
        {
            this.Headers.ContentLength = null;
            this._result = result;
            this._jsonSerializer = jsonSerializer;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            //StreamWriter writer = new StreamWriter(stream);
            //writer.Write(JsonConvert.SerializeObject(_result));
            //await writer.FlushAsync();

            var writer = new StreamWriter(stream);
            this._jsonSerializer.Serialize(writer, _result);
            await writer.FlushAsync();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 1;
            return true;
        }
    }
}
