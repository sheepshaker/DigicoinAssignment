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
        Broker GetBroker(string brokerId);
        Client GetClient(string clientId);
        IList<Order> Orders { get; }
        decimal GetClientNetPosition(string clientId);
        void AddClient(string clientId);
        void RemoveClient(string clientId);
        void AddBroker(string brokerId, IDictionary<int, decimal> commissionMap, decimal price);
        void RemoveBroker(string brokerId);
    }
}
