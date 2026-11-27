using EventBus.Base;
using EventBus.Base.Events;

namespace EventBus.UnitTest
{
    //transport free bus for base class tests
    public class TestEventBus : BaseEventBus
    {
        public TestEventBus(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
        }

        public override void Publish(IntegrationEvent @event)
        {
        }

        public override void Subscribe<T, TH>()
        {
            _subsManager.AddSubscription<T, TH>();
        }

        public override void UnSubscribe<T, TH>()
        {
            _subsManager.RemoveSubscription<T, TH>();
        }
    }
}
