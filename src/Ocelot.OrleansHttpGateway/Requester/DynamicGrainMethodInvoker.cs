using Newtonsoft.Json;
using Ocelot.Logging;
using Ocelot.OrleansHttpGateway.Infrastructure;
using Ocelot.OrleansHttpGateway.Model;
using Ocelot.Responses;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Ocelot.OrleansHttpGateway.Requester
{
    internal class DynamicGrainMethodInvoker : IGrainMethodInvoker
    {
        private readonly IParameterBinder _parameterBinder;
        private readonly ConcurrentDictionary<string, ObjectMethodExecutor> _cachedExecutors = new ConcurrentDictionary<string, ObjectMethodExecutor>();
        private readonly IOcelotLogger _logger;
        private readonly JsonSerializer _jsonSerializer;

        public DynamicGrainMethodInvoker(IParameterBinder parameterBinder, IOcelotLoggerFactory factory,
            JsonSerializer jsonSerializer)
        {
            this._parameterBinder = parameterBinder;
            this._logger = factory.CreateLogger<DynamicGrainMethodInvoker>();
            this._jsonSerializer = jsonSerializer;

        }

        public async Task<Response<OrleansResponseMessage>> Invoke(GrainReference grain, GrainRouteValues route)
        {
            ObjectMethodExecutor executor;
            object[] parameters;
            try
            {
                string key = $"{route.SiloName}.{route.GrainName}.{route.GrainMethodName}";
                executor = _cachedExecutors.GetOrAdd(key, (_key) =>
                {
                    ObjectMethodExecutor _executor = ObjectMethodExecutor.Create(route.GrainMethod, grain.GrainType.GetTypeInfo());
                    return _executor;
                });
                parameters = GetParameters(executor, route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new ErrorResponse<OrleansResponseMessage>(new UnknownError(ex.Message));
            }
            var result = await this.ExecuteAsync(executor, grain, parameters);
            var message = new OrleansResponseMessage(new OrleansContent(result, this._jsonSerializer), HttpStatusCode.OK);
            return new OkResponse<OrleansResponseMessage>(message);
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
                var param = _parameterBinder.BindParameters(executor.MethodParameters, route);
                if (param.Length == executor.MethodParameters.Length)
                    return param;
                else
                    throw new UnableToFindDownstreamRouteException("The request parameter is inconsistent with the parameter that executes the action.");
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