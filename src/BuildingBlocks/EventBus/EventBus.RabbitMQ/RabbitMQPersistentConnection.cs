using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IDisposable
    {
        private IConnection _connection;
        private readonly IConnectionFactory connectionFactory;
        private readonly int retryCount;
        public bool IsConnected => _connection != null && _connection.IsOpen;
        private object lock_object = new object();
        private bool _disposed;

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            this.connectionFactory = connectionFactory;
            this.retryCount = retryCount;
        }

        public async Task<IChannel> CreateModelAsync()
        {
            if (!IsConnected)
                throw new InvalidOperationException("RabbitMQ connection is not available.");

            return await _connection!.CreateChannelAsync();
        }

        public void Dispose()
        {
            _disposed = true;
            _connection.Dispose();
        }

        public async Task<bool> TryConnectAsync()
        {
            if (_disposed)
                return false;

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(retryCount, retryAttempt => 
                    TimeSpan.FromSeconds(
                        Math.Pow(2, retryAttempt)),
                                (ex, time) =>
                                {
                                    // Log
                                });

            try
            {
                await policy.ExecuteAsync(async () =>
                {
                    _connection =
                        await connectionFactory.CreateConnectionAsync();
                });

                if (IsConnected)
                {
                    _connection!.ConnectionShutdownAsync += OnConnectionShutdownAsync;

                    _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;

                    _connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;

                    return true;
                }
            }
            catch (Exception)
            {
                // Log connection failure
            }

            return false;
        }

        private async Task OnConnectionBlockedAsync(object sender, ConnectionBlockedEventArgs @event)
        {
            if (!_disposed)
            {
                await Task.Run(() => TryConnectAsync());
            }
        }

        private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            if (!_disposed)
            {
                await Task.Run(() => TryConnectAsync());
            }
        }

        private async Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
        {
            if (!_disposed)
            {
                await Task.Run(() => TryConnectAsync());
            }
        }
    }
}
