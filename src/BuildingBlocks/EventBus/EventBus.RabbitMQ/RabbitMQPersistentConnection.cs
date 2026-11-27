using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IDisposable
    {
        private IConnection? _connection;
        private readonly IConnectionFactory connectionFactory;
        private readonly int retryCount;
        private readonly ILogger logger;
        private readonly SemaphoreSlim connectLock = new SemaphoreSlim(1, 1);
        private bool _disposed;

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5, ILogger? logger = null)
        {
            this.connectionFactory = connectionFactory;
            this.retryCount = retryCount;
            this.logger = logger ?? NullLogger.Instance;
        }

        public async Task<IChannel> CreateModelAsync()
        {
            if (!IsConnected)
                throw new InvalidOperationException("RabbitMQ connection is not available.");

            return await _connection!.CreateChannelAsync();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "RabbitMQ connection could not be disposed");
            }
        }

        public async Task<bool> TryConnectAsync()
        {
            if (_disposed)
                return false;

            //only one connect attempt at a time
            await connectLock.WaitAsync();

            try
            {
                if (IsConnected)
                    return true;

                logger.LogInformation("RabbitMQ client is trying to connect");

                //retry policy for broker connection
                var policy = Policy
                    .Handle<BrokerUnreachableException>()
                    .Or<SocketException>()
                    .WaitAndRetryAsync(retryCount,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (ex, time) =>
                        {
                            logger.LogWarning(ex, "RabbitMQ client could not connect after {TimeOut}s", $"{time.TotalSeconds:n1}");
                        });

                await policy.ExecuteAsync(async () =>
                {
                    _connection = await connectionFactory.CreateConnectionAsync();
                });

                if (IsConnected)
                {
                    _connection!.ConnectionShutdownAsync += OnConnectionShutdownAsync;
                    _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;
                    _connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;

                    logger.LogInformation("RabbitMQ client acquired a persistent connection to {HostName}", _connection.Endpoint.HostName);

                    return true;
                }

                logger.LogCritical("RabbitMQ connection could not be created and opened");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "RabbitMQ connection failed");
            }
            finally
            {
                connectLock.Release();
            }

            return false;
        }

        //client auto recovery handles reconnect, events only logged
        private Task OnConnectionBlockedAsync(object sender, ConnectionBlockedEventArgs @event)
        {
            if (!_disposed)
                logger.LogWarning("RabbitMQ connection is blocked: {Reason}", @event.Reason);

            return Task.CompletedTask;
        }

        private Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            if (!_disposed)
                logger.LogWarning("RabbitMQ connection is shut down: {ReplyText}", e.ReplyText);

            return Task.CompletedTask;
        }

        private Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
        {
            if (!_disposed)
                logger.LogWarning(e.Exception, "RabbitMQ connection threw a callback exception");

            return Task.CompletedTask;
        }
    }
}
