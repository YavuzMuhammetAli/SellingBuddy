using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregateModels.OrderAggregate
{
    public class Address : ValueObject
    {
        public string Street { get; private set; }

        public string City { get; private set; }

        public string State { get; private set; }

        public string Country { get; private set; }

        public string ZipCode { get; private set; }

        public string ContactName { get; private set; }

        public string Phone { get; private set; }

        //used by ef core
        public Address()
        {
        }

        public Address(string street, string city, string state, string country, string zipCode, string contactName = null, string phone = null)
        {
            ContactName = contactName;
            Phone = phone;
            Street = street;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return State;
            yield return Country;
            yield return ZipCode;
            yield return ContactName;
            yield return Phone;
        }
    }
}
