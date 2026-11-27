using Microsoft.AspNetCore.Components;
using StatusDashboard.Application;
using StatusDashboard.Core.Models;

namespace StatusDashboard.Components.Shared
{
    //pages re-render when the monitor writes a new snapshot
    public abstract class LiveComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        protected StatusStore Store { get; set; }

        protected DashboardSnapshot Snapshot => Store.Current;

        protected override void OnInitialized()
        {
            Store.Changed += OnStoreChanged;
        }

        private void OnStoreChanged()
        {
            _ = InvokeAsync(StateHasChanged);
        }

        public virtual void Dispose()
        {
            Store.Changed -= OnStoreChanged;
        }
    }
}
