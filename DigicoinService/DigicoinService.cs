using System;
using System.Collections.Generic;
using System.Linq;
using DigicoinService.Model;

namespace DigicoinService
{
    public class DigicoinService : IDigicoinService
    {
        private readonly IDictionary<string, Broker> _brokers;
        private readonly IDictionary<string, IList<AllocatedOrder>> _orders;

        public DigicoinService()
        {
            _orders = new Dictionary<string, IList<AllocatedOrder>>();
            _brokers = new Dictionary<string, Broker>();

            InitialiseBrokers();
        }

        private void InitialiseBrokers()
        {
            Dictionary<int, decimal> commisionMap = new Dictionary<int, decimal>();
            for (int i = 10; i <= 100; i += 10)
            {
                commisionMap.Add(i, 0.05m);
            }
            var broker1 = new Broker("Broker 1", commisionMap, 1.49m);

            commisionMap = new Dictionary<int, decimal>();
            for (int i = 10; i <= 100; i += 10)
            {
                if (i >= 10 && i <= 40)
                {
                    commisionMap.Add(i, 0.03m);

                }
                else if (i >= 50 && i <= 80)
                {
                    commisionMap.Add(i, 0.025m);

                }
                else
                {
                    commisionMap.Add(i, 0.02m);
                }
            }
            var broker2 = new Broker("Broker 2", commisionMap, 1.52m);

            _brokers.Add("Broker 1", broker1);
            _brokers.Add("Broker 2", broker2);
        }

        private IEnumerable<Quote> GetBestQuotes(int lotSize)
        {
            Dictionary<Broker, IEnumerable<Quote>> quoteMap = new Dictionary<Broker, IEnumerable<Quote>>();

            foreach (var broker in _brokers.Values)
            {
                quoteMap[broker] = broker.GetQuotes(lotSize);
            }

            //generate cartesian product from all the quotes of all the brokers
            //this will give us all the combinations of split orders
            var res = quoteMap.Values.CartesianProduct().
                //filter out order sizes which don't match the required lotSize
                Where(array => array.Sum(quote => quote.LotSize) == lotSize).
                //create sorted dictionary with the value contating collection of quotes and associated price as the key
                ToDictionary(quote => quote.Sum(q => q.PriceAfterCommission)).OrderBy(dict => dict.Key);

            //take the collection of quotes for the best price, ignore dummy quotes, sort by Price
            return res.FirstOrDefault().Value.Where(v => v.IsEmpty == false).OrderBy(q => q.PriceAfterCommission);
        }

        public decimal ExecuteOrder(string clientId, Order order)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("clientId");
            }

            var bestQuotes = GetBestQuotes(order.LotSize).ToArray();

            var price = AllocateOrder(clientId, order, bestQuotes);

            return price;
        }

        private decimal AllocateOrder(string clientId, Order order, Quote[] quotes)
        {
            decimal totalPrice = 0;

            foreach (var quote in quotes)
            {
                _brokers[quote.BrokerId].AddVolume(quote.LotSize);
                totalPrice += quote.PriceAfterCommission;
            }

            AllocatedOrder ao = new AllocatedOrder(order, totalPrice, quotes);

            if (_orders.ContainsKey(clientId) == false)
            {
                _orders.Add(clientId, new List<AllocatedOrder> {ao});
            }
            else
            {
                _orders[clientId].Add(ao);
            }

            return totalPrice;
        }

        public Dictionary<string, decimal> ClientsNetPosition
        {
            get { return _orders.ToDictionary(ao => ao.Key, ao => CalculateClientsNetPosition(ao.Value.ToArray())); }
        }

        public Dictionary<string, int> BrokersVolumeTraded
        {
            get { return _brokers.ToDictionary(b => b.Key, b => b.Value.VolumeTraded); }
        }

        private decimal CalculateClientsNetPosition(AllocatedOrder[] allocatedOrders)
        {
            var netPositions = allocatedOrders.Average(o => o.OrderPrice/Math.Abs(o.ClientOrder.LotSize))*
                               allocatedOrders.Sum(o => o.VolumeTraded);

            //test output seem to be 3 DP max, e.g.: 296.156 as opposed to 296.1564103
            netPositions = Math.Round(netPositions, 3);

            return netPositions;
        }
    }
}
