using NServiceBus;

namespace Shipping
{
    public class ShippingPolicyData : ContainSagaData
    {
        public string OrderId { get; set; }
        public bool IsOrderPkaced { get; set; }

        public bool IsOrderBilled { get; set; }
    }
}