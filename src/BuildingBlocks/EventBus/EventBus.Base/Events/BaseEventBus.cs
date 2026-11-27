using EventBus.Base.Abstraction;
using EventBus.Base.SubManager;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        public readonly IEventBusSubscriptionManager _subsManager;
        public IServiceProvider _serviceProvider;

        public EventBusConfig eventBusConfig { get; set; }

        public BaseEventBus(EventBusConfig config, IServiceProvider serviceProvider) 
        {
            eventBusConfig = config;
            _serviceProvider = serviceProvider;
            _subsManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
        }

        public virtual string ProcessEventName(string eventName)
        {
            //prefix and suffix removed as whole strings, trim removed chars one by one
            if (eventBusConfig.DeleteEventPrefix && eventName.StartsWith(eventBusConfig.EventNamePrefix))
                eventName = eventName.Substring(eventBusConfig.EventNamePrefix.Length);

            if (eventBusConfig.DeleteEventSuffix && eventName.EndsWith(eventBusConfig.EventNameSuffix))
                eventName = eventName.Substring(0, eventName.Length - eventBusConfig.EventNameSuffix.Length);

            return eventName;
        }

        public virtual string GetSubName(string eventName)
        {
            return $"{eventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }

        public virtual void Dispose()
        {
            _subsManager.Clear();
        }

        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);

            var processed = false;

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);

                using(var scope = _serviceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        //handler resolved from message scope
                        var handler = scope.ServiceProvider.GetService(subscription.HandleType);

                        if (handler == null) continue;

                        var eventType = _subsManager.GetEventTypeByName($"{eventBusConfig.EventNamePrefix}{eventName}{eventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });

                        processed = true;
                    }
                }
            }

            return processed;
        }

        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        public abstract void UnSubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
}
