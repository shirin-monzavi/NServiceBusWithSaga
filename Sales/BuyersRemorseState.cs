using NServiceBus;

namespace Sales
{
    public class BuyersRemorseState : ContainSagaData
    {
        public string OrderId { get; set; }
    }
}