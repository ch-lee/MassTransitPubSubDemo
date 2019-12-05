using System;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransitExample.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransitExample.Publisher.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // https://stackoverflow.com/questions/49434228/how-to-specify-which-azure-service-bus-topic-to-use-with-masstransit

            string connectionString = "Endpoint=sb://demo-xyz.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=";

            string flightOrdersTopic = "flight-orders";

            // create the bus using Azure Service bus
            var azureServiceBus = Bus.Factory.CreateUsingAzureServiceBus(busFactoryConfig =>
            {
                // specify the message FlightOrder to be sent to a specific topic
                busFactoryConfig.Message<FlightOrder>(configTopology =>
                {
                    configTopology.SetEntityName(flightOrdersTopic);
                });

                var host = busFactoryConfig.Host(connectionString, hostConfig =>
                {
                    // This is optional, but you can specify the protocol to use.
                    hostConfig.TransportType = TransportType.AmqpWebSockets;
                });

            });

            // optional: Can register queue names with messages
//            EndpointConvention.Map<FlightCancellation>(new Uri($"sb://demo-xyz.servicebus.windows.net/flight-cancellation"));

            // Add MassTransit
            services.AddMassTransit(config => 
            {
                config.AddBus(provider => azureServiceBus);
            });

            // register MassTransit's IPublishEndpoint, ISendEndpointProvider and IBus which can be used to send and publish messages
            services.AddSingleton<IPublishEndpoint>(azureServiceBus);
            services.AddSingleton<ISendEndpointProvider>(azureServiceBus);
            services.AddSingleton<IBus>(azureServiceBus);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
