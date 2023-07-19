using Messages;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading.Tasks;

namespace Shipping
{
    public class ShipWithAlpineHanlder : IHandleMessages<ShipWithAlpine>
    {
        private static ILog log = LogManager.GetLogger<ShipWithAlpineHanlder>();

        private const int MaximumTimeAlpineMightRespond = 30;
        private static Random random = new Random();

        public async Task Handle(ShipWithAlpine message, IMessageHandlerContext context)
        {
            var waitingTime = random.Next(MaximumTimeAlpineMightRespond);

            log.Info($"ShipWithAlpineHandler: Delaying Order [{message.OrderId}] {waitingTime} seconds.");

            await Task.Delay(waitingTime * 1000);

            await context.Reply(new ShipmentAcceptedByAlpine());
        }
    }
}