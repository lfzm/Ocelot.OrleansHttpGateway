using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ocelot.OrleansHttpGateway.Infrastructure
{
    internal static class ReflectionUtil
    {
        public static IEnumerable<MethodInfo> GetMethodsIncludingBaseInterfaces(Type t)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance;


            foreach (var mi in t.GetMethods())
            {
                yield return mi;
            }
            
            foreach (Type interf in t.GetInterfaces())
            {
                foreach (MethodInfo method in interf.GetMethods(flags))
                    yield return method;
            }
        }

        public static Type GetAnyElementType(Type type)
        {
            // Type is Array
            // short-circuit if you expect lots of arrays 
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            // type implements/extends IEnumerable<T>;
            var enumType = type.GetInterfaces()
                .Where(t => t.IsGenericType &&
                            t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
            return enumType ?? type;
        }

        public static Func<object,object> GetObjectActivator(Type type, Type arg1)
        {
            var ctor = type.GetConstructor(new[] {arg1});

            ParameterExpression param =
                Expression.Parameter(typeof(object), "arg");

            Expression[] argsExp =
            {
                Expression.Convert(param, arg1)
            };

            NewExpression newExp = Expression.New(ctor, argsExp);

            var convert = Expression.Convert(newExp, typeof(object));

            // Create a lambda with the New expression as body and our param object[] as arg
            LambdaExpression lambda = Expression.Lambda(typeof(Func<object,object>), convert, param);

            // Compile it
           return  (Func<object, object>)lambda.Compile();
        }

        public static Func<object, object> GetValueGetter(Type type,string propertyName)
        {
            var propertyInfo = type.GetProperty(propertyName);

            var instance = Expression.Parameter(typeof(object), "i");
            
            var property = Expression.Property(Expression.Convert(instance,type), propertyInfo);
            var convert = Expression.Convert(property, typeof(object));
            return (Func<object, object>)Expression.Lambda(convert, instance).Compile();
        }
    }
}