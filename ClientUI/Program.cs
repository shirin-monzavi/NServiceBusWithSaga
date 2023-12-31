﻿using Messages;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading.Tasks;

namespace ClientUI
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.Title = "ClientUI";

            var endpointConfiguration = new EndpointConfiguration("ClientUI");

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");
            routing.RouteToEndpoint(typeof(CancellOrder), "Sales");

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            await RunLoop(endpointInstance)
                .ConfigureAwait(false);

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }

        private static ILog log = LogManager.GetLogger<Program>();

        private static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            var lastOrder = string.Empty;
            while (true)
            {
                log.Info("Press 'P' to place an order, 'C' to cancel last order, or 'Q' to quit.");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.P:
                        // Instantiate the command
                        var command = new PlaceOrder
                        {
                            OrderId = Guid.NewGuid().ToString()
                        };

                        // Send the command
                        log.Info($"Sending PlaceOrder command, OrderId = {command.OrderId}");
                        await endpointInstance.Send(command)
                            .ConfigureAwait(false);

                        lastOrder = command.OrderId;

                        break;

                    case ConsoleKey.C:
                        var cancelCommand = new CancellOrder()
                        {
                            OrderId = lastOrder
                        };

                        await endpointInstance.Send(cancelCommand).ConfigureAwait(false);
                        log.Info($"Sent a correlated message to {cancelCommand.OrderId}");
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        log.Info("Unknown input. Please try again.");
                        break;
                }
            }
        }
    }
}