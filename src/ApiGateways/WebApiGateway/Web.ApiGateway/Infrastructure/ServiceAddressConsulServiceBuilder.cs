using Consul;
using Ocelot.Logging;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Consul.Interfaces;

namespace Web.ApiGateway.Infrastructure
{
    //default builder uses the consul node name as host, services register their own address
    public class ServiceAddressConsulServiceBuilder : DefaultConsulServiceBuilder
    {
        public ServiceAddressConsulServiceBuilder(IHttpContextAccessor contextAccessor, IConsulClientFactory clientFactory, IOcelotLoggerFactory loggerFactory)
            : base(contextAccessor, clientFactory, loggerFactory)
        {
        }

        protected override string GetDownstreamHost(ServiceEntry entry, Node node) => entry.Service.Address;
    }
}
