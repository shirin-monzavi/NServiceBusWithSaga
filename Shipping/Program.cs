using Messages;
using NServiceBus;
using System;
using System.Threading.Tasks;

namespace Shipping
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.Title = "Shipping";

            var endpointConfiguration = new EndpointConfiguration("Shipping");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(ShipOrder), "Shipping");
            routing.RouteToEndpoint(typeof(ShipWithMaple), "Shipping");
            routing.RouteToEndpoint(typeof(ShipWithAlpine), "Shipping");

            var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}