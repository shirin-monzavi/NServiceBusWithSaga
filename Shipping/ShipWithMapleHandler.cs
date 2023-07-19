using Messages;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading.Tasks;

namespace Shipping
{
    internal class ShipWithMapleHandler : IHandleMessages<ShipWithMaple>
    {
        private static ILog log = LogManager.GetLogger<ShipWithMapleHandler>();

        private const int MaximumTimeMapleMightRespond = 60;
        private static Random random = new Random();

        public async Task Handle(ShipWithMaple message, IMessageHandlerContext context)
        {
            var waitingTime = random.Next(MaximumTimeMapleMightRespond);
            log.Info($"ShipWithMapleHandler Delaying order {message.OrderId} {waitingTime} seconds");

            await Task.Delay(waitingTime * 1000);
            await context.Reply(new ShipmentAcceptedByMaple());
        }
    }
}