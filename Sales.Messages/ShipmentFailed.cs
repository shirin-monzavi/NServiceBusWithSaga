using NServiceBus;

namespace Messages
{
    public class ShipmentFailed : IEvent
    {
        public string OrderId { get; set; }
    }
}