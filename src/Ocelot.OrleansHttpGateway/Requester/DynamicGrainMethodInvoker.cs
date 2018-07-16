using Ocelot.OrleansHttpGateway.Infrastructure;
using Ocelot.OrleansHttpGateway.Model;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Ocelot.OrleansHttpGateway.Requester
{
    internal class DynamicGrainMethodInvoker : IGrainMethodInvoker
    {
        private readonly IParameterBinder _parameterBinder;
        private readonly ConcurrentDictionary<string, ObjectMethodExecutor> _cachedExecutors = new ConcurrentDictionary<string, ObjectMethodExecutor>();

        public DynamicGrainMethodInvoker(IParameterBinder parameterBinder)
        {
            _parameterBinder = parameterBinder;

        }

        public async Task<object> Invoke(GrainReference grain, GrainRouteValues route)
        {
            string key = $"{route.SiloName}.{route.GrainName}.{route.GrainMethodName}";
            var executor = _cachedExecutors.GetOrAdd(key, (_key) =>
             {
                 ObjectMethodExecutor _executor = ObjectMethodExecutor.Create(route.GrainMethod, grain.GrainType.GetTypeInfo());
                 return _executor;
             });

            var parameters = GetParameters(executor, route);
            return await this.ExecuteAsync(executor, grain, parameters);
        }

        private object[] GetParameters(ObjectMethodExecutor executor, GrainRouteValues route)
        {
            //short circuit if no parameters
            if (executor.MethodParameters == null || executor.MethodParameters.Length == 0)
            {
                return Array.Empty<object>();
            }
            // loop through binders, in order
            // first suitable binder wins
            // so the order of registration is important
            ExceptionDispatchInfo lastException = null;
            try
            {
                return _parameterBinder.BindParameters(executor.MethodParameters, route);
            }
            catch (Exception ex)
            {
                // continue on next suitable binder
                // but keep the exception when no other suitable binders are found
                lastException = ExceptionDispatchInfo.Capture(ex);
            }
            lastException?.Throw();
            return Array.Empty<object>();

        }

        private async Task<object> ExecuteAsync(ObjectMethodExecutor executor, GrainReference grain, object[] parameters)
        {
            try
            {
                return await executor.ExecuteAsync(grain.Grain, parameters);
            }
            catch (Exception ex)
            {
                throw new OrleansRequestException("Requesting Orleans failed ...", ex);
            }
        }

    }


}