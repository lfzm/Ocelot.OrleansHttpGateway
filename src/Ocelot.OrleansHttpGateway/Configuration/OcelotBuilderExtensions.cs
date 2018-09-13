using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Ocelot.Configuration.Repository;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Configuration;
using Ocelot.OrleansHttpGateway.Infrastructure;
using Ocelot.OrleansHttpGateway.Requester;
using Ocelot.ServiceDiscovery;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var configuration = builder.Services.SingleOrDefault(s => s.ServiceType.Name == typeof(IConfiguration).Name)?.ImplementationInstance as IConfiguration;
            if (configuration == null)
                throw new OrleansHttpGateway.Requester.OrleansConfigurationException("can't find Orleans section in appsetting.json");
            configuration = builder.Configuration.GetSection("Orleans");
            if (configuration == null)
                throw new OrleansHttpGateway.Requester.OrleansConfigurationException("can't find Orleans section in appsetting.json");
            builder.Services.Configure<OrleansHttpGatewayOptions>(configuration);
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

            //Orleans need to use ServiceName configuration, configure ServiceName must have a service discovery provider, component forgery one
            if (services.Where(s => s.ServiceType.Name == typeof(ServiceDiscoveryFinderDelegate).Name).Count() == 0)
                services.TryAddSingleton<ServiceDiscoveryFinderDelegate>((provider, config, name) => throw new Exception($"Unable to find service discovery provider for type: {config.Type}"));
            return services;
        }
    }
}
