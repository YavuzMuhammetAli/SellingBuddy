using EventBus.Base;
using EventBus.Base.Events;
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
        RabbitMQPersistentConnection rabbitMQPersistentConnection;
        private readonly IConnectionFactory connectionFactory;
        private readonly IChannel consumeChannel;

        public EventBusRabbitMQ(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            if(config.Connection != null)
            {
                var connctionJSON = JsonConvert.SerializeObject(eventBusConfig.Connection, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connctionJSON);
            }
            else
                connectionFactory = new ConnectionFactory();

            rabbitMQPersistentConnection = new RabbitMQPersistentConnection(connectionFactory, config.ConnectionRetryCount);

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
            if(!rabbitMQPersistentConnection.IsConnected)
                rabbitMQPersistentConnection.TryConnectAsync().GetAwaiter().GetResult();

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(
                    eventBusConfig.ConnectionRetryCount, 
                    retryAttempt => TimeSpan.FromSeconds(
                        Math.Pow(2, retryAttempt)), (ex, time) =>
                             {
                                 // Log
                             });

            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            consumeChannel.ExchangeDeclareAsync(exchange: eventBusConfig.DefaultTopicName, type: "direct").GetAwaiter().GetResult();

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = new BasicProperties
                {
                    DeliveryMode = DeliveryModes.Persistent
                };
               
                consumeChannel.BasicPublishAsync(
                    exchange: eventBusConfig.DefaultTopicName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body).GetAwaiter().GetResult();

                consumeChannel.BasicPublishAsync(
                    exchange: eventBusConfig.DefaultTopicName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body).GetAwaiter().GetResult();
            });
        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if(!_subsManager.HasSubscriptionsForEvent(eventName))
            {
                if(!rabbitMQPersistentConnection.IsConnected)
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

            _subsManager.AddSubscription<T, TH>();
            StartBasicConsumeChannel(eventName);
        }

        public override void UnSubscribe<T, TH>()
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        private IChannel CreateConsumeChannel()
        {
            if(!rabbitMQPersistentConnection.IsConnected)
                rabbitMQPersistentConnection.TryConnectAsync().GetAwaiter().GetResult();

            var channel = rabbitMQPersistentConnection.CreateModelAsync().GetAwaiter().GetResult();

            channel.ExchangeDeclareAsync(exchange: eventBusConfig.DefaultTopicName, type: "direct").GetAwaiter().GetResult();

            return channel;
        }

        private void StartBasicConsumeChannel(string eventName)
        {
            if(consumeChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(consumeChannel);
                
                consumer.ReceivedAsync += Consumer_Received;
                
                consumeChannel.BasicConsumeAsync(
                    queue: GetSubName(eventName),
                    autoAck: false,
                    consumer: consumer).GetAwaiter().GetResult();
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            eventName = ProcessEventName(eventName);

            var message = Encoding.UTF8.GetString(@event.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
            }

            await consumeChannel.BasicAckAsync(@event.DeliveryTag, multiple: false);
        }
    }
}
