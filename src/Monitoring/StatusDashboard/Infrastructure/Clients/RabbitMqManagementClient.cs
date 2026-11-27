using StatusDashboard.Core.Models;
using System.Text.Json;

namespace StatusDashboard.Infrastructure.Clients
{
    public class RabbitMqManagementClient
    {
        private readonly HttpClient _httpClient;

        public RabbitMqManagementClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BrokerOverview> GetOverviewAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var document = JsonDocument.Parse(await _httpClient.GetStringAsync("api/overview", cancellationToken));
                var root = document.RootElement;

                return new BrokerOverview
                {
                    Reachable = true,
                    ClusterName = GetString(root, "cluster_name"),
                    RabbitMqVersion = GetString(root, "rabbitmq_version"),
                    ErlangVersion = GetString(root, "erlang_version"),
                    Connections = (int)GetLong(root, "object_totals", "connections"),
                    Channels = (int)GetLong(root, "object_totals", "channels"),
                    Consumers = (int)GetLong(root, "object_totals", "consumers"),
                    Queues = (int)GetLong(root, "object_totals", "queues"),
                    MessagesReady = GetLong(root, "queue_totals", "messages_ready"),
                    MessagesUnacknowledged = GetLong(root, "queue_totals", "messages_unacknowledged"),
                    PublishRate = GetDouble(root, "message_stats", "publish_details", "rate"),
                    DeliverRate = GetDouble(root, "message_stats", "deliver_get_details", "rate")
                };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                return new BrokerOverview { Reachable = false, Error = ex is TaskCanceledException ? "Timed out" : ex.Message };
            }
        }

        public async Task<List<QueueInfo>> GetQueuesAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var document = JsonDocument.Parse(await _httpClient.GetStringAsync("api/queues", cancellationToken));

                return document.RootElement.EnumerateArray().Select(q =>
                {
                    var name = GetString(q, "name");

                    //app names can contain dots, the event is after the last one
                    var separator = name.LastIndexOf('.');

                    return new QueueInfo
                    {
                        Name = name,
                        OwnerService = separator > 0 ? name[..separator] : name,
                        EventName = separator > 0 ? name[(separator + 1)..] : string.Empty,
                        MessagesReady = GetLong(q, "messages_ready"),
                        MessagesUnacknowledged = GetLong(q, "messages_unacknowledged"),
                        Consumers = (int)GetLong(q, "consumers"),
                        State = GetString(q, "state"),
                        PublishRate = GetDouble(q, "message_stats", "publish_details", "rate"),
                        DeliverRate = GetDouble(q, "message_stats", "deliver_get_details", "rate")
                    };
                }).OrderBy(q => q.OwnerService).ThenBy(q => q.EventName).ToList();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                return new List<QueueInfo>();
            }
        }

        private static bool TryGet(JsonElement element, out JsonElement value, params string[] path)
        {
            value = element;

            foreach (var key in path)
            {
                if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(key, out value))
                    return false;
            }

            return true;
        }

        private static string GetString(JsonElement element, params string[] path) =>
            TryGet(element, out var value, path) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

        private static long GetLong(JsonElement element, params string[] path) =>
            TryGet(element, out var value, path) && value.ValueKind == JsonValueKind.Number ? value.GetInt64() : 0;

        private static double GetDouble(JsonElement element, params string[] path) =>
            TryGet(element, out var value, path) && value.ValueKind == JsonValueKind.Number ? Math.Round(value.GetDouble(), 2) : 0;
    }
}
