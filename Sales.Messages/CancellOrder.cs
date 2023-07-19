using NServiceBus;

namespace Messages
{
    public class CancellOrder : ICommand
    {
        public string OrderId { get; set; }
    }
}