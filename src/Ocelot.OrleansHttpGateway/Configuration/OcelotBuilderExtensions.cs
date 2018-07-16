using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
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
        public static IOcelotBuilder AddOrleansHttpGateway(this IOcelotBuilder builder, Action<OrleansHttpGatewayOptions> configure, Action<OrleansRequesterConfiguration> OrleansRequesterConfiguration = null)
        {
            builder.Services.AddOrleansHttpGateway(OrleansRequesterConfiguration);
            builder.Services.Configure<OrleansHttpGatewayOptions>(configure);
            return builder;
        }
        public static IOcelotBuilder AddOrleansHttpGateway(this IOcelotBuilder builder, Action<OrleansRequesterConfiguration> OrleansRequesterConfiguration = null)
        {
            builder.Services.AddOrleansHttpGateway(OrleansRequesterConfiguration);
        
            builder.Services.Configure<OrleansHttpGatewayOptions>(builder.Configuration.GetSection("Orleans"));
            return builder;
        }

        private static IServiceCollection AddOrleansHttpGateway(this IServiceCollection services, Action<OrleansRequesterConfiguration> OrleansRequesterConfiguration = null)
        {
            if (OrleansRequesterConfiguration != null)
                services.Configure<OrleansRequesterConfiguration>(OrleansRequesterConfiguration);
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

            services.TryAddSingleton<IOrleansAuthorisation, DefaultOrleansAuthorisation>();
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
