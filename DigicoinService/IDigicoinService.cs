using System.Collections.Generic;
using DigicoinService.Model;

namespace DigicoinService
{
    public interface IDigicoinService
    {
        Dictionary<string, decimal> ClientsNetPosition { get; }
        Dictionary<string, int> BrokersVolumeTraded { get; } 
        decimal ExecuteOrder(string clientId, Order order);
    }
}
