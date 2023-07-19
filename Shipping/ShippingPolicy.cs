using Messages;
using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;

namespace Shipping
{
    public class ShippingPolicy : Saga<ShippingPolicyData>,
        IAmStartedByMessages<OrderPlaced>,//start saga
        IAmStartedByMessages<OrderBilled>//
    {
        private static ILog log = LogManager.GetLogger<ShippingPolicy>();

        public Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            log.Info($"Received orderPlaced, OrderId={message.OrderId}");
            Data.IsOrderBilled = true;
            return ProcessOrder(context);
        }

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            log.Info($"Received OrderBilled, OrderId={message.OrderId}");
            Data.IsOrderPkaced = true;
            return ProcessOrder(context);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
        {
            mapper.MapSaga(sagaData => sagaData.OrderId)
        .ToMessage<OrderPlaced>(message => message.OrderId)
        .ToMessage<OrderBilled>(message => message.OrderId);
        }

        private async Task ProcessOrder(IMessageHandlerContext context)
        {
            if (Data.IsOrderBilled && Data.IsOrderPkaced)
            {
                await context.SendLocal(new ShipOrder() { OrderId = Data.OrderId });
                MarkAsComplete();
            }
        }
    }
}