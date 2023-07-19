using Messages;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading.Tasks;

namespace Sales
{
    public class BuyersRemorsePolicy : Saga<BuyersRemorseState>,
        IAmStartedByMessages<PlaceOrder>,
        IHandleTimeouts<BuyersRemorseIsOver>,
        IHandleMessages<CancellOrder>

    {
        private static ILog log = LogManager.GetLogger<BuyersRemorsePolicy>();

        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            log.Info($"Received PlaceOrder, OrderId= {message.OrderId}");
            Data.OrderId = message.OrderId;
            log.Info($"starting cool down perios for oder {Data.OrderId}");

            await RequestTimeout(context, TimeSpan.FromSeconds(1), new BuyersRemorseIsOver());
        }

        public Task Handle(CancellOrder message, IMessageHandlerContext context)
        {
            log.Info($"Order {message.OrderId} was cancelled");

            MarkAsComplete();
            return Task.CompletedTask;
        }

        public async Task Timeout(BuyersRemorseIsOver state, IMessageHandlerContext context)
        {
            log.Info($" cooling down for order {Data.OrderId} has elapsed");

            var orderPlace = new OrderPlaced()
            {
                OrderId = Data.OrderId,
            };

            await context.Publish(orderPlace);

            MarkAsComplete();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BuyersRemorseState> mapper)
        {
            mapper.MapSaga(sagaData => sagaData.OrderId)
                 .ToMessage<PlaceOrder>(message => message.OrderId)
                 .ToMessage<CancellOrder>(message => message.OrderId);
        }
    }

    public class BuyersRemorseIsOver
    {
        public BuyersRemorseIsOver()
        {
        }
    }
}