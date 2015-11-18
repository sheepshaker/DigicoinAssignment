using System;
using System.Collections.Generic;
using System.Linq;
using DigicoinService.Model;

namespace DigicoinService
{
    public class DigicoinService : IDigicoinService
    {
        private IEnumerable<Client> _clients;
        private IEnumerable<Broker> _brokers;

        private DigicoinService()
        {
            //hide default constructor
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

            _clients = clients;
            _brokers = brokers;
        }

        public IEnumerable<Client> Clients
        {
            get { return _clients; }
        }

        public IEnumerable<Broker> Brokers
        {
            get { return _brokers; }
        }

        public Order Buy(string clientId, int lotSize)
        {
            return CreateNewOrder(Direction.Buy, clientId, lotSize);
        }

        public Order Sell(string clientId, int lotSize)
        {
            return CreateNewOrder(Direction.Sell, clientId, lotSize);
        }

        private IEnumerable<Quote> GetBestQuotes(int lotSize)
        {
            Dictionary<Broker, IEnumerable<Quote>> quoteMap = new Dictionary<Broker, IEnumerable<Quote>>();
            
            foreach(var broker in Brokers)
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
            
            //take the collection of quotes for the best price, ignore dummy quotes
            return res.FirstOrDefault().Value.Where(v => v.IsEmpty == false);
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

            Client client = _clients.FirstOrDefault(c => c.UserId == clientId);

            if (client == null)
            {
                throw new ArgumentException("Client doesn't exist", "clientId");
            }

            var bestQuotes = GetBestQuotes(lotSize);

            var order = new Order(direction, bestQuotes);

            client.AddOrder(order);

            foreach (var quote in order.Quotes)
            {
                quote.Broker.AddVolume(quote.LotSize);
            }

            return order;
        }
    }
}
