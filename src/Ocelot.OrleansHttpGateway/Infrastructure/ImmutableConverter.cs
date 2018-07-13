using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.Concurrency;
using System;
using System.Collections.Concurrent;

namespace Ocelot.OrleansHttpGateway.Infrastructure
{
    public class ImmutableConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Immutable<>))
                return true;

            return false;
        }

        public override bool CanRead { get; } = true;
        public override bool CanWrite { get; } = true;


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var valueType = GetValueType(objectType);

            // need to see if this is formatted as
            // 'regular' json, thus with a Value: property
            var value = token.ToObject(valueType, serializer);


            return GetImmutableTInstance(objectType, value);
        }

        static readonly ConcurrentDictionary<Type, Func<object, object>> _activatorCache = new ConcurrentDictionary<Type, Func<object, object>>();

        static object GetImmutableTInstance(Type type, object value)
        {
            var activator = _activatorCache.GetOrAdd(type, (t) =>
                ReflectionUtil.GetObjectActivator(type, GetValueType(type)));

            return activator(value);
        }

        static Type GetValueType(Type immutableType)
        {
            return immutableType.GetGenericArguments()[0];
        }

        static readonly ConcurrentDictionary<Type, Func<object, object>> _valueGetterCache = new ConcurrentDictionary<Type, Func<object, object>>();


        static object GetValue(object immutableType)
        {
            var getter = _valueGetterCache.GetOrAdd(immutableType.GetType(), type =>ReflectionUtil.GetValueGetter(type,"Value"));
            return getter(immutableType);
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var token = JToken.FromObject(GetValue(value),serializer);
            token.WriteTo(writer);
        }
    }
}