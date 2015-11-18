using System;
using System.Collections.Generic;
using System.Linq;
using DigicoinService.Model;

namespace DigicoinService
{
    public class DigicoinService : IDigicoinService
    {
        private readonly IDictionary<string, Client> _clients;
        private readonly IDictionary<string, Broker> _brokers;
        private readonly IList<Order> _orders;

        public DigicoinService()
        {
            _clients = new Dictionary<string, Client>();
            _brokers = new Dictionary<string, Broker>();
            _orders = new List<Order>();
        }

        public DigicoinService(IEnumerable<Client> clients, IEnumerable<Broker> brokers) 
        {
            if (clients == null)
            {
                throw new ArgumentNullException("clients");
            }

            if (brokers == null)
            {
                throw new ArgumentNullException("brokers");
            }

            _clients = clients.ToDictionary(c => c.UserId);
            _brokers = brokers.ToDictionary(b => b.UserId);
            _orders = new List<Order>();
        }

        public IEnumerable<Client> Clients
        {
            get { return _clients.Values; }
        }

        public IEnumerable<Broker> Brokers
        {
            get { return _brokers.Values; }
        }

        public Order Buy(string clientId, int lotSize)
        {
            return CreateNewOrder(Direction.Buy, clientId, lotSize);
        }

        public Order Sell(string clientId, int lotSize)
        {
            return CreateNewOrder(Direction.Sell, clientId, lotSize);
        }

        /*private IEnumerable<Quote> GetBestQuotes(int lotSize)
        {
            List<Quote> quotes = new List<Quote>();
            Dictionary<Broker, IEnumerable<Quote>> quoteMap = new Dictionary<Broker, IEnumerable<Quote>>();
            Dictionary<string, Broker> brokersMap = Brokers.ToDictionary(b => b.UserId);

            List<int> split = new List<int>();

            if (lotSize > 100)
            {
                split.Add(lotSize - 100);
                split.Add(100);
            }
            else
            {
                split.Add(lotSize);
            }

            foreach (var s in split)
            {
                foreach (var broker in brokersMap.Values)
                {
                    quoteMap[broker] = broker.GetQuotes(s);
                }

                //generate cartesian product from all the quotes of all the brokers
                //this will give us all the combinations of split orders
                var res = quoteMap.Values.CartesianProduct().
                    //filter out order sizes which don't match the required lotSize
                    Where(array => array.Sum(quote => quote.LotSize) == lotSize).
                    //create sorted dictionary with the value contating collection of quotes and associated price as the key
                    ToDictionary(quote => quote.Sum(q => q.Price)).OrderBy(dict => dict.Key);

                //take the collection of quotes for the best price, ignore dummy quotes
                var bestQuotes = res.FirstOrDefault().Value.FirstOrDefault(v => v.IsEmpty == false);

                quotes.Add(bestQuotes);

                brokersMap.Remove(bestQuotes.BrokerId);
            }

            return quotes;
        }*/

        private IEnumerable<Quote> GetBestQuotes(int lotSize)
        {
            Dictionary<Broker, IEnumerable<Quote>> quoteMap = new Dictionary<Broker, IEnumerable<Quote>>();

            foreach (var broker in Brokers)
            {
                quoteMap[broker] = broker.GetQuotes(lotSize);
            }

            //generate cartesian product from all the quotes of all the brokers
            //this will give us all the combinations of split orders
            var res = quoteMap.Values.CartesianProduct().
                //filter out order sizes which don't match the required lotSize
                Where(array => array.Sum(quote => quote.LotSize) == lotSize).
                //create sorted dictionary with the value contating collection of quotes and associated price as the key
                ToDictionary(quote => quote.Sum(q => q.Price)).OrderBy(dict => dict.Key);

            var x = res.FirstOrDefault().Value.Where(v => v.IsEmpty == false).OrderBy(o => o.Price).ToList();
            //take the collection of quotes for the best price, ignore dummy quotes, sort by Price
            return res.FirstOrDefault().Value.Where(v => v.IsEmpty == false).OrderBy(o => o.Price);
        }

        private Order CreateNewOrder(Direction direction, string clientId, int lotSize)
        {
            if (lotSize > 200)
            {
                throw new ArgumentOutOfRangeException("lotSize", "Invalid size: no more than 100 per Broker");
            }

            if (lotSize % 10 > 0)
            {
                throw new ArgumentException("Invalid increment: only multiples of 10", "lotSize");
            }

            if (lotSize < 10)
            {
                throw new ArgumentOutOfRangeException("lotSize", "Invalid size: 10 is minimum");
            }

            if (_clients.ContainsKey(clientId) == false)
            {
                throw new ArgumentException("Client doesn't exist", "clientId");
            }

           

            var bestQuotes = GetBestQuotes(lotSize).ToList();
            //quotes.AddRange(bestQuotes.ToArray());
            

            var order = new Order(direction, clientId, bestQuotes);

            if (order.Quotes != null)
            {
                foreach (var quote in order.Quotes)
                {
                    _brokers[quote.BrokerId].AddVolume(quote.LotSize);
                }

            }

            _orders.Add(order);
           
            return order;
        }

        public Broker GetBroker(string brokerId)
        {
            return Brokers.FirstOrDefault(b => b.UserId == brokerId);
        }

        public Client GetClient(string clientId)
        {
            return Clients.FirstOrDefault(c => c.UserId == clientId);
        }

        public IList<Order> Orders
        {
            get
            {
                return _orders;
            }
        }

        public decimal GetClientNetPosition(string clientId)
        {
            if (_clients.ContainsKey(clientId) == false)
            {
                throw new ArgumentException("Client doesn't exist", "clientId");
            }

            if (_orders.Any() == false)
            {
                throw new Exception("No orders available");
            }

            var orders = _orders.Where(o => o.ClientId == clientId).ToArray();
            if (orders.Any() == false)
            {
                throw new Exception(string.Format("No orders available for client {0}", clientId));
            }

            var netPositions = orders.Average(o => o.TotalPrice/Math.Abs(o.TotalVolume))*
                                   orders.Sum(o => o.TotalVolume);

            //test output seem to be 3 DP max, e.g.: 296.156 as opposed to 296.1564103
            netPositions = Math.Round(netPositions, 3);

            return netPositions;
        }

        public void AddClient(string clientId)
        {
            if (_clients.ContainsKey(clientId))
            {
                throw new ArgumentException("Client already exists", "clientId");
            }

            _clients.Add(clientId, new Client(clientId));
        }

        public void RemoveClient(string clientId)
        {
            if (_clients.ContainsKey(clientId))
            {
                throw new ArgumentException("Client doesn't exists", "clientId");
            }

            _clients.Remove(clientId);
        }

        public void AddBroker(string brokerId, IDictionary<int, decimal> commissionMap, decimal price)
        {
            if (_brokers.ContainsKey(brokerId))
            {
                throw new ArgumentException("Broker already exists", "brokerId");
            }

            _brokers.Add(brokerId, new Broker(brokerId, commissionMap, price));
        }

        public void RemoveBroker(string brokerId)
        {
            if (_brokers.ContainsKey(brokerId))
            {
                throw new ArgumentException("Broker doesn't exists", "brokerId");
            }

            _brokers.Remove(brokerId);
        }

    }
}
