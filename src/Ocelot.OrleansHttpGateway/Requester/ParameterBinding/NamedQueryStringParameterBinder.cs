using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Ocelot.Orleans.Core;
using Ocelot.Orleans.Infrastructure;

namespace Ocelot.Orleans.Core.ParameterBinding
{
    class NamedQueryStringParameterBinder : IParameterBinder
    {
        readonly JsonSerializer _serializer;

        public NamedQueryStringParameterBinder(JsonSerializer serializer)
        {
            this._serializer = serializer;
        }

        public Task<bool> CanBind(ParameterInfo[] parameters, HttpRequest request)
        {
            if (parameters.Length == request.Query.Count)
            {
                //check parameter names
                var source = parameters.Select(x => x.Name).ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
                return Task.FromResult(source.SetEquals(request.Query.Select(x => x.Key)));
            }

            return Task.FromResult(false);
        }

        public Task<object[]> BindParameters(ParameterInfo[] parameters, HttpRequest request)
        {
            var result = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                //support named parameters in querystring
                if (request.Query.TryGetValue(parameter.Name, out StringValues value))
                {
                    if (parameter.ParameterType.IsArray)
                    {
                        var elementType = ReflectionUtil.GetAnyElementType(parameter.ParameterType);
                        Array array = Array.CreateInstance(elementType, value.Count);
                        for (int p = 0; p < value.Count; p++)
                        {
                            array.SetValue( Convert(value[p], elementType),p);
                        }
                        result[i] = array;
                    }
                    else
                    {
                        result[i] = Convert(value[0], parameter.ParameterType);
                    }
                }
            }
            return Task.FromResult(result);
        }

        object Convert(string source, Type t)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(t);
            if (converter.CanConvertTo(t) && converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFromString(source);
            }
            else if (t == typeof(Guid))
            {
                return Guid.Parse(source);
            }

            //fallback to json serializer..
            using (var jsonTextReader = new JsonTextReader(new StringReader(source)))
            {
                return _serializer.Deserialize(jsonTextReader, t);
            }
        }
    }


}