using Microsoft.Extensions.Options;
using StatusDashboard.Core.Models;
using StatusDashboard.Core.Options;

namespace StatusDashboard.Application
{
    //singleton shared by every circuit, written only by StatusMonitorService
    public class StatusStore
    {
        private const int MaxChanges = 50;

        private readonly object _lock = new();
        private readonly int _historyLength;
        private readonly Dictionary<string, Queue<StatusSample>> _history = new();
        private readonly LinkedList<StatusChange> _changes = new();
        private readonly Dictionary<string, OverallStatus> _lastStatus = new();
        private readonly SemaphoreSlim _refreshSignal = new(0, 1);

        public StatusStore(IOptions<MonitoringOptions> options)
        {
            _historyLength = Math.Max(5, options.Value.HistoryLength);
            RefreshSeconds = Math.Max(2, options.Value.RefreshSeconds);
        }

        public int RefreshSeconds { get; }

        public DashboardSnapshot Current { get; private set; } = DashboardSnapshot.Empty;

        public event Action Changed;

        public event Action<StatusChange> StatusChanged;

        public static string ServiceKey(string name) => $"service:{name}";

        public static string InfrastructureKey(string name) => $"infra:{name}";

        public IReadOnlyList<StatusSample> GetHistory(string key)
        {
            lock (_lock)
                return _history.TryGetValue(key, out var queue) ? queue.ToList() : new List<StatusSample>();
        }

        public IReadOnlyList<StatusChange> GetRecentChanges(int count = 10)
        {
            lock (_lock)
                return _changes.Take(count).ToList();
        }

        //manual refresh from the ui
        public void RequestRefresh()
        {
            if (_refreshSignal.CurrentCount == 0)
            {
                try { _refreshSignal.Release(); }
                catch (SemaphoreFullException) { }
            }
        }

        public Task WaitForRefreshAsync(TimeSpan timeout, CancellationToken cancellationToken) =>
            _refreshSignal.WaitAsync(timeout, cancellationToken);

        public void Update(DashboardSnapshot snapshot)
        {
            var newChanges = new List<StatusChange>();

            lock (_lock)
            {
                foreach (var service in snapshot.Services)
                    Track(ServiceKey(service.Name), service.Name, service.Status, service.LatencyMs, snapshot.UpdatedAt, newChanges);

                foreach (var infra in snapshot.Infrastructure)
                    Track(InfrastructureKey(infra.Name), infra.Name, infra.Status, infra.LatencyMs, snapshot.UpdatedAt, newChanges);

                Current = snapshot;
            }

            Changed?.Invoke();

            foreach (var change in newChanges)
                StatusChanged?.Invoke(change);
        }

        private void Track(string key, string name, OverallStatus status, double? latency, DateTime time, List<StatusChange> newChanges)
        {
            if (!_history.TryGetValue(key, out var queue))
                _history[key] = queue = new Queue<StatusSample>();

            queue.Enqueue(new StatusSample { Time = time, Status = status, LatencyMs = latency });

            while (queue.Count > _historyLength)
                queue.Dequeue();

            //first sample is the baseline, not a change
            if (_lastStatus.TryGetValue(key, out var previous) && previous != status)
            {
                var change = new StatusChange { Time = time, Target = name, From = previous, To = status };

                _changes.AddFirst(change);
                newChanges.Add(change);

                while (_changes.Count > MaxChanges)
                    _changes.RemoveLast();
            }

            _lastStatus[key] = status;
        }
    }
}
