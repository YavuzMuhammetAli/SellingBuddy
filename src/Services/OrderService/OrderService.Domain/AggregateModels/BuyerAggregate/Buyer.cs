using OrderService.Domain.Events;
using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregateModels.BuyerAggregate
{
    public class Buyer : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; }

        private readonly List<PaymentMethod> paymentMethods;

        public IEnumerable<PaymentMethod> PaymentMethods => paymentMethods.AsReadOnly();

        //used by ef core
        protected Buyer()
        {
            paymentMethods = new List<PaymentMethod>();
        }

        public Buyer(string name) : this()
        {
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
        }

        public PaymentMethod VerifyOrAddPaymentMethod(int cardTypeId, string alias, string cardNumber, string securityNumber,
            string cardHolderName, DateTime expiration, Guid orderId)
        {
            //same card reused instead of adding a new one
            var existingPayment = paymentMethods.SingleOrDefault(p => p.IsEqualTo(cardTypeId, cardNumber, expiration));

            if (existingPayment != null)
            {
                AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, existingPayment, orderId));
                return existingPayment;
            }

            var payment = new PaymentMethod(cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);

            paymentMethods.Add(payment);

            AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId));

            return payment;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj) || (obj is Buyer buyer && Id.Equals(buyer.Id) && Name == buyer.Name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
