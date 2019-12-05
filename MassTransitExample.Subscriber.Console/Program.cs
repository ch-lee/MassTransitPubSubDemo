using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransitExample.Messages;
using MassTransitExample.Subscriber.Console.Consumers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransitExample.Subscriber.Console
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(config =>
                    {
                        config.AddConsumer<FlightPurchasedConsumer>();
                        config.AddConsumer<FlightCancellationConsumer>();
                        config.AddBus(ConfigureBus);
                    });

                    services.AddSingleton<IHostedService, MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                });

            

            await builder.RunConsoleAsync();
        }

        static IBusControl ConfigureBus(IServiceProvider provider)
        {
            string connectionString =
                "Endpoint=sb://demo-xyz.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=";

            string flightOrdersTopic = "flight-orders";

            string subscriptionName = "flight-subscriber-01";

            string queueName = "flight-cancellation";

            
            var azureServiceBus = Bus.Factory.CreateUsingAzureServiceBus(busFactoryConfig =>
            {

                busFactoryConfig.Message<FlightOrder>(m => { m.SetEntityName(flightOrdersTopic); });

                var host = busFactoryConfig.Host(connectionString, hostConfig => 
                {
                    hostConfig.TransportType = TransportType.AmqpWebSockets;
                });

                // setup Azure topic consumer
                busFactoryConfig.SubscriptionEndpoint<FlightOrder>(host, subscriptionName, configurator =>
                {
                    configurator.Consumer<FlightPurchasedConsumer>(provider);
                });


                // setup Azure queue consumer
                busFactoryConfig.ReceiveEndpoint(host, queueName, configurator =>
                {
                    configurator.Consumer<FlightCancellationConsumer>(provider);

                    // as this is a queue, no need to subscribe to topics, so set this to false.
                    // configurator.SubscribeMessageTopics = false;
                });
            });


            return azureServiceBus;
        }
    }
}
