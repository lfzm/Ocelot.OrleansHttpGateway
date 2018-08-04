using Ocelot.DownstreamRouteFinder.Finder;
using Ocelot.Logging;
using Ocelot.OrleansHttpGateway.Infrastructure;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Responses;
using Orleans;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Ocelot.OrleansHttpGateway.Requester
{
    internal class DefaultGrainReference : IGrainReference
    {
        private readonly ConcurrentDictionary<string, Func<string, object>> _GrainReferenceCache = new ConcurrentDictionary<string, Func<string, object>>(StringComparer.OrdinalIgnoreCase);
        private readonly Tuple<Type, MethodInfo>[] _grainIdentityInterfaceMap =
         typeof(IGrainFactory).GetMethods()
             .Where(x => x.Name == "GetGrain" && x.IsGenericMethod)
             .Select(x => Tuple.Create(x.GetGenericArguments()[0].GetGenericParameterConstraints()[0], x)).ToArray();

        private readonly IGrainFactoryProxy _grainProxy;
        private readonly IOcelotLogger _logger;

        public DefaultGrainReference(IGrainFactoryProxy grainProxy, IOcelotLoggerFactory factory)
        {
            this._grainProxy = grainProxy;
            this._logger = factory.CreateLogger<DefaultGrainReference>();
        }

        public Response<GrainReference> GetGrainReference(GrainRouteValues route)
        {
            try
            {
                var grainFunc = _GrainReferenceCache.GetOrAdd(route.GrainType.FullName, key =>
                    {
                        return this.BuildFactoryMethod(route.GrainType);
                    });
                var grain = grainFunc(route.GrainId);
                var grainReference = new GrainReference(route.GrainType, grain);
                return new OkResponse<GrainReference>(grainReference);
            }
            catch (UnableToFindDownstreamRouteException ex)
            {
                this._logger.LogWarning(ex.Message);
                return new ErrorResponse<GrainReference>(new UnableToFindDownstreamRouteError(route.RequestUri, "orleans"));
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message,ex);
                return new ErrorResponse<GrainReference>(new UnknownError(ex.Message));
            }
        }

        private Func<string, object> BuildFactoryMethod(Type grainType)
        {
            var mi = _grainIdentityInterfaceMap.FirstOrDefault(x => x.Item1.IsAssignableFrom(grainType));
            if (mi != null)
            {
                var factoryDelegate = DelegateFactory.Create(mi.Item2.GetGenericMethodDefinition().MakeGenericMethod(grainType));
                //Construct the GrainKey parameter
                var idParts = GetArgumentParser(mi.Item2.GetParameters());
                return (id) => factoryDelegate(_grainProxy, idParts(id));
            }
            throw new OrleansGrainReferenceException($"cannot construct grain {grainType.Name}");
        }

        private Func<string, object[]> GetArgumentParser(ParameterInfo[] parameters)
        {
            string[] idseperator = new[] { "," };
            return (id) =>
            {
                if (string.IsNullOrEmpty(id))
                {
                    if (parameters.Where(f => f.ParameterType == typeof(long)).Count() > 0)
                    {
                        //Random negative number
                        Random rd = new Random();
                        id = rd.Next(-1000000, 0).ToString();
                    }
                    else if (parameters.Where(f => f.ParameterType == typeof(Guid)).Count() > 0)
                        id = Guid.NewGuid().ToString();
                    else
                    {
                        Random rd = new Random();
                        id = "ocelot_#"+ rd.Next(0,100000);
                    }
                     
                }

                var idParts = id.Split(idseperator, StringSplitOptions.RemoveEmptyEntries);
                object[] values = new object[parameters.Length];
                for (int i = 0; i < idParts.Length; i++)
                {

                    values[i] = TryParse(idParts[i], parameters[i].ParameterType);
                }
                return values;
            };
        }
        static object TryParse(string source, Type t)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(t);
            if (converter.CanConvertTo(t) && converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFromString(source);
            }
            else if (t == typeof(Guid))
            {
                if (Guid.TryParse(source, out Guid result))
                    return result;
                else
                    throw new UnableToFindDownstreamRouteException($"Guid \"{source}\" Parameter Grain primary Key is incorrect");
            }
            throw new OrleansGrainReferenceException($"Can't parse '{ nameof(source)}' as {t.FullName}");
        }
    }
}
