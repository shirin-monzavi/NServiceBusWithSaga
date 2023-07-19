using Messages;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading.Tasks;

namespace Shipping
{
    internal class ShipOrderWorkFlow :
        Saga<ShipOrderWorkFlow.ShipOrdeData>,
        IAmStartedByMessages<ShipOrder>,
        IHandleMessages<ShipmentAcceptedByMaple>,
        IHandleTimeouts<ShipOrderWorkFlow.ShippingEsclation>,
        IHandleMessages<ShipmentAcceptedByAlpine>

    {
        private static ILog log = LogManager.GetLogger<ShipOrderWorkFlow>();

        public async Task Handle(ShipOrder message, IMessageHandlerContext context)
        {
            log.Info($"ShipOrderWorkFlow for order {message.OrderId}  trying maple first");
            await context.Send(new ShipWithMaple()
            {
                OrderId = message.OrderId,
            });

            await RequestTimeout(context, TimeSpan.FromSeconds(20), new ShippingEsclation());
        }

        public Task Handle(ShipmentAcceptedByMaple message, IMessageHandlerContext context)
        {
            if (!Data.ShipmentOrderSentToAlpine)
            {
                log.Info($"Order {Data.OrderId} - successfully shipped with Maple");
                Data.ShipmentAcceptedByMaple = true;
                MarkAsComplete();
            }

            return Task.CompletedTask;
        }

        public Task Handle(ShipmentAcceptedByAlpine message, IMessageHandlerContext context)
        {
            log.Info($"Order {Data.OrderId} successfully shipped with Alpine");
            Data.ShipmentAcceptedByAlpine = true;
            MarkAsComplete();

            return Task.CompletedTask;
        }

        public async Task Timeout(ShippingEsclation state, IMessageHandlerContext context)
        {
            if (!Data.ShipmentAcceptedByMaple)
            {
                if (!Data.ShipmentOrderSentToAlpine)
                {
                    log.Info($"Order {Data.OrderId} - we did not receive answer from maple lets try alpine");
                    Data.ShipmentOrderSentToAlpine = true;
                    await context.Send(new ShipWithAlpine() { OrderId = Data.OrderId, });

                    await RequestTimeout(context, TimeSpan.FromSeconds(20), new ShippingEsclation());
                }
                else if (!Data.ShipmentAcceptedByAlpine)
                {
                    log.Warn($"Order {Data.OrderId} no naswer from maple/alpine we need to escalate!");
                    await context.Publish<ShipmentFailed>();
                    MarkAsComplete();
                }
            }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShipOrdeData> mapper)
        {
            mapper.MapSaga(sagaData => sagaData.OrderId)
                  .ToMessage<ShipOrder>(message => message.OrderId);
        }

        internal class ShipOrdeData : ContainSagaData
        {
            public string OrderId { get; set; }

            public bool ShipmentAcceptedByMaple { get; set; }

            public bool ShipmentOrderSentToAlpine { get; set; }

            public bool ShipmentAcceptedByAlpine { get; set; }
        }

        internal class ShippingEsclation
        {
        }
    }
}