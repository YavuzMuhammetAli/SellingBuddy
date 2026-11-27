using MediatR;

namespace OrderService.Domain.SeedWork
{
    public abstract class BaseEntity
    {
        public virtual Guid Id { get; protected set; }

        //id created with the entity, ef core never generates it
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
        }

        public DateTime CreateDate { get; protected set; } = DateTime.UtcNow;

        private List<INotification> domainEvents;

        public IReadOnlyCollection<INotification> DomainEvents => domainEvents?.AsReadOnly();

        public void AddDomainEvent(INotification eventItem)
        {
            domainEvents ??= new List<INotification>();
            domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            domainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            domainEvents?.Clear();
        }

        public bool IsTransient()
        {
            return Id == default;
        }

        public override bool Equals(object obj)
        {
            if (obj is not BaseEntity item)
                return false;

            if (ReferenceEquals(this, item))
                return true;

            if (GetType() != item.GetType())
                return false;

            if (item.IsTransient() || IsTransient())
                return false;

            return item.Id == Id;
        }

        public override int GetHashCode()
        {
            return IsTransient() ? base.GetHashCode() : Id.GetHashCode() ^ 31;
        }

        public static bool operator ==(BaseEntity left, BaseEntity right)
        {
            return Equals(left, null) ? Equals(right, null) : left.Equals(right);
        }

        public static bool operator !=(BaseEntity left, BaseEntity right)
        {
            return !(left == right);
        }
    }
}
