using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Ocelot.DependencyInjection;
using Ocelot.OrleansHttpGateway.Configuration;
using Ocelot.OrleansHttpGateway.Infrastructure;
using Ocelot.OrleansHttpGateway.Requester;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using System;
using System.Linq;

namespace Ocelot.DependencyInjection
{
    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddOrleansHttpGateway(this IOcelotBuilder builder, Action<OrleansHttpGatewayOptions> configure)
        {
            builder.Services.AddOrleansHttpGateway();
            builder.Services.Configure<OrleansHttpGatewayOptions>(options =>
            {
                configure?.Invoke(options);
            });
            return builder;
        }
        public static IOcelotBuilder AddOrleansHttpGateway(this IOcelotBuilder builder)
        {
            builder.Services.AddOrleansHttpGateway();
            builder.Services.Configure<OrleansHttpGatewayOptions>(builder.Configuration.GetSection("Orleans"));
            return builder;
        }

        private static IServiceCollection AddOrleansHttpGateway(this IServiceCollection services)
        {
            //JsonSerializer 
            services.AddSingleton<JsonSerializer>((IServiceProvider serviceProvider) =>
            {
                JsonSerializerSettings settings = serviceProvider.GetService<JsonSerializerSettings>()
                    ?? new JsonSerializerSettings();

                if (!settings.Converters.OfType<ImmutableConverter>().Any())
                {
                    settings.Converters.Add(new ImmutableConverter());
                }
                return JsonSerializer.Create(settings);
            });

            services.TryAddSingleton<IOrleansRequester, DefaultOrleansRequester>();
            services.TryAddSingleton<IClusterClientBuilder, DefaultClusterClientBuilder>();
            services.TryAddSingleton<IGrainFactoryProxy, DefaultGrainFactoryProxy>();
            services.TryAddSingleton<IGrainMethodInvoker, DynamicGrainMethodInvoker>();
            services.TryAddSingleton<IGrainReference, DefaultGrainReference>();
            services.TryAddSingleton<IRouteValuesBuilder, DefaultRouteValuesBuilder>();
            services.TryAddSingleton<IParameterBinder, DefaultParameterBinder>();
            return services;
        }
    }
}
