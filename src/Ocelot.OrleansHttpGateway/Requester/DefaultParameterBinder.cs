using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.OrleansHttpGateway.Infrastructure;
using Ocelot.OrleansHttpGateway.Model;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;


namespace Ocelot.OrleansHttpGateway.Requester
{
    public class DefaultParameterBinder : IParameterBinder
    {
        readonly JsonSerializer _serializer;
        public DefaultParameterBinder(JsonSerializer serializer)
        {
            this._serializer = serializer;
        }
        public object[] BindParameters(ParameterInfo[] parameters, GrainRouteValues routeValues)
        {
            if (parameters == null || parameters.Length <= 0)
                return Array.Empty<object>();


            var result = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                try
                {
                    var param = parameters[i];
                    object value = null;
                    if (param.ParameterType.CanHaveChildren())
                    {
                        value = this.BindClassType(param, routeValues.Body);
                    }
                    else
                        value = this.BindPrimitiveType(param, routeValues.Querys, routeValues.Body);

                    if (value == null)
                        return new object[0];

                    result[i] = value;
                }
                catch (Exception ex)
                {
                    throw new OrleansGrainReferenceException("Bind this parameters data failed", ex);
                }
            }

            return result;
        }

        public object BindPrimitiveType(ParameterInfo parameter, IQueryCollection queryData, JObject bodyData)
        {
            if (queryData.TryGetValue(parameter.Name, out StringValues value))
            {
                return Convert(value, parameter.ParameterType);
            }
            else if (bodyData.TryGetValue(parameter.Name, StringComparison.OrdinalIgnoreCase, out JToken qvalue))
            {
                return qvalue.ToObject(parameter.ParameterType, _serializer);
            }
            else
                return null;
        }


        public object BindClassType(ParameterInfo parameter, JObject bodyData)
        {
            if (bodyData.HasValues && bodyData.TryGetValue(parameter.Name, StringComparison.OrdinalIgnoreCase, out JToken qvalue))
            {
                return qvalue.ToObject(parameter.ParameterType, _serializer);
            }
            else if (bodyData.HasValues)
            {
                return bodyData.ToObject(parameter.ParameterType, _serializer);
            }
            else
                return null;
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