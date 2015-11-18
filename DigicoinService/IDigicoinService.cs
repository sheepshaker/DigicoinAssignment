using System.Collections.Generic;
using DigicoinService.Model;

namespace DigicoinService
{
    public interface IDigicoinService
    {
        IEnumerable<Client> Clients { get; }
        IEnumerable<Broker> Brokers { get; }
        Order Buy(string clientId, int lotSize);
        Order Sell(string clientId, int lotSize);
    }
}
