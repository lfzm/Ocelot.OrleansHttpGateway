using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.OrleansHttpGateway.Configuration;

namespace OcelotOrleans.AspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
           .UseKestrel()
           .UseContentRoot(Directory.GetCurrentDirectory())
           .ConfigureAppConfiguration((hostingContext, config) =>
           {
               config
                    .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                    .AddJsonFile("ocelot.json")
                    .AddEnvironmentVariables();
           })
           .ConfigureServices(s =>
           {
               s.AddAuthentication()
                    .AddJwtBearer("uc", x =>
                   {
                       x.RequireHttpsMetadata = false;
                       x.Authority = "http://auth.zop.alingfly.com/";
                       x.Audience = "COTC_API";
                   })
                   .AddJwtBearer("cotc", x =>
                   {
                       x.RequireHttpsMetadata = false;
                       x.Authority = "http://auth.zop.alingfly.com/";
                       x.Audience = "COTC_API";
                   });

               s.AddOcelot()
                    .AddOrleansHttpGateway((OrleansRequesterConfiguration config) =>
                    {
                        config.MapRouteToGraininterface = (route) =>
                        {
                            return "I{GrainName}Service".Replace("{GrainName}", route.GrainName);
                        };
                        config.RequestContextInjection = (context) =>
                        {
                            Orleans.Runtime.RequestContext.Set("Client-IP", context.HttpContext.Connection.RemoteIpAddress.ToString());
                            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues value))
                                Orleans.Runtime.RequestContext.Set("Authorization", value);
                        };
                    });
           })
           .ConfigureLogging((hostingContext, logging) =>
           {
               //add your logging
               logging.AddConsole();
           })
           .UseIISIntegration()
           .Configure(app =>
           {
               app.UseOcelot(config =>
               {
                   config.AddOrleansHttpGateway();
               }).Wait();
           })
           .Build()
           .Run();
        }


    }
}
