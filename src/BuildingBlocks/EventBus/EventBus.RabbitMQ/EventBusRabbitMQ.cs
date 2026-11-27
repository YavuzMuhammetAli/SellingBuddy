using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        private readonly RabbitMQPersistentConnection rabbitMQPersistentConnection;
        private readonly IConnectionFactory connectionFactory;
        private readonly IChannel consumeChannel;
        private readonly ILogger<EventBusRabbitMQ> logger;

        public EventBusRabbitMQ(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<EventBusRabbitMQ>>() ?? NullLogger<EventBusRabbitMQ>.Instance;

            //factory can be passed directly or as a plain settings object
            if (config.Connection is IConnectionFactory factory)
            {
                connectionFactory = factory;
            }
            else if (config.Connection != null)
            {
                var connectionJson = JsonConvert.SerializeObject(eventBusConfig.Connection, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connectionJson) ?? new ConnectionFactory();
            }
            else
                connectionFactory = new ConnectionFactory();

            rabbitMQPersistentConnection = new RabbitMQPersistentConnection(connectionFactory, config.ConnectionRetryCount, logger);

            consumeChannel = CreateConsumeChannel();

            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object? sender, string eventName)
        {
            eventName = ProcessEventName(eventName);

            if (!rabbitMQPersistentConnection.IsConnected)
                rabbitMQPersistentConnection.TryConnectAsync().GetAwaiter().GetResult();

            consumeChannel.QueueUnbindAsync(
                queue: GetSubName(eventName),
                exchange: eventBusConfig.DefaultTopicName,
                routingKey: eventName).GetAwaiter().GetResult();

            if (_subsManager.IsEmpty)
                consumeChannel.CloseAsync().GetAwaiter().GetResult();
        }

        public override void Publish(IntegrationEvent @event)
        {
            if (!rabbitMQPersistentConnection.IsConnected)
                rabbitMQPersistentConnection.TryConnectAsync().GetAwaiter().GetResult();

            //retry policy for publish
            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(
                    eventBusConfig.ConnectionRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        logger.LogWarning(ex, "Could not publish event {EventId} after {Timeout}s", @event.Id, $"{time.TotalSeconds:n1}");
                    });

            var eventName = ProcessEventName(@event.GetType().Name);

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                //separate channel for publish, channels are not thread safe
                using var channel = rabbitMQPersistentConnection.CreateModelAsync().GetAwaiter().GetResult();

                channel.ExchangeDeclareAsync(exchange: eventBusConfig.DefaultTopicName, type: "direct").GetAwaiter().GetResult();

                var properties = new BasicProperties
                {
                    DeliveryMode = DeliveryModes.Persistent
                };

                logger.LogInformation("Publishing event {EventName} ({EventId}) to RabbitMQ", eventName, @event.Id);

                channel.BasicPublishAsync(
                    exchange: eventBusConfig.DefaultTopicName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body).GetAwaiter().GetResult();
            });
        }

        public override void Subscribe<T, TH>()
        {
            var eventName = ProcessEventName(typeof(T).Name);
            var isFirstSubscription = !_subsManager.HasSubscriptionsForEvent(eventName);

            if (isFirstSubscription)
            {
                if (!rabbitMQPersistentConnection.IsConnected)
                    rabbitMQPersistentConnection.TryConnectAsync().GetAwaiter().GetResult();

                consumeChannel.QueueDeclareAsync(
                    queue: GetSubName(eventName),
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null).GetAwaiter().GetResult();

                consumeChannel.QueueBindAsync(
                    queue: GetSubName(eventName),
                    exchange: eventBusConfig.DefaultTopicName,
                    routingKey: eventName).GetAwaiter().GetResult();
            }

            logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

            _subsManager.AddSubscription<T, TH>();

            //one consumer per queue
            if (isFirstSubscription)
                StartBasicConsume(eventName);
        }

        public override void UnSubscribe<T, TH>()
        {
            logger.LogInformation("Unsubscribing from event {EventName}", typeof(T).Name);

            _subsManager.RemoveSubscription<T, TH>();
        }

        public override void Dispose()
        {
            base.Dispose();

            consumeChannel?.Dispose();
            rabbitMQPersistentConnection.Dispose();
        }

        private IChannel CreateConsumeChannel()
        {
            if (!rabbitMQPersistentConnection.IsConnected)
                rabbitMQPersistentConnection.TryConnectAsync().GetAwaiter().GetResult();

            var channel = rabbitMQPersistentConnection.CreateModelAsync().GetAwaiter().GetResult();

            channel.ExchangeDeclareAsync(exchange: eventBusConfig.DefaultTopicName, type: "direct").GetAwaiter().GetResult();

            return channel;
        }

        private void StartBasicConsume(string eventName)
        {
            if (consumeChannel == null)
            {
                logger.LogError("StartBasicConsume can't be called on a null consume channel");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(consumeChannel);

            consumer.ReceivedAsync += Consumer_Received;

            consumeChannel.BasicConsumeAsync(
                queue: GetSubName(eventName),
                autoAck: false,
                consumer: consumer).GetAwaiter().GetResult();
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = ProcessEventName(@event.RoutingKey);
            var message = Encoding.UTF8.GetString(@event.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message);

                await consumeChannel.BasicAckAsync(@event.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message {EventName}: {Message}", eventName, message);

                //failed message rejected so it doesn't loop forever
                await consumeChannel.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }
}
