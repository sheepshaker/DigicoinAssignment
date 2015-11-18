using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DigicoinService;
using DigicoinService.Model;

namespace DigicoinTest
{
    [TestClass]
    public class DefaultDigiconTests
    {
        private IDigicoinService _service;

        [TestInitialize]
        public void Init()
        {
            List<Client> clients = new List<Client>();
            clients.Add(new Client("Client A"));
            clients.Add(new Client("Client B"));
            clients.Add(new Client("Client C"));

            List<Broker> brokers = new List<Broker>();
            Dictionary<int, decimal> commisionMap = new Dictionary<int, decimal>();
            for (int i = 10; i <= 100; i += 10)
            {
                commisionMap.Add(i, 0.05m);
            }

            brokers.Add(new Broker("Broker 1", commisionMap, 1.49m));

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
            brokers.Add(new Broker("Broker 2", commisionMap, 1.52m));

            _service = new DigicoinService.DigicoinService(clients.ToArray(), brokers.ToArray());
        }

        [TestMethod]
        public void DefaultTests()
        {
            IEnumerable<Order> transactions = new List<Order>
            {
                new Order(Direction.Buy, "Client A", new[] {new Quote(10, 15.6450m, "Broker 1")}),
                new Order(Direction.Buy, "Client B", new[] {new Quote(40, 62.58m, "Broker 1")}),
                new Order(Direction.Buy, "Client A", new[] {new Quote(50, 77.9m, "Broker 2")}),
                new Order(Direction.Buy, "Client B", new[] {new Quote(100, 155.04m, "Broker 2")}),
                new Order(Direction.Sell, "Client B", new[] {new Quote(80, 124.64m, "Broker 2")}),
                new Order(Direction.Sell, "Client C", new[] {new Quote(70, 109.06m, "Broker 2")}),
                new Order(Direction.Buy, "Client A", new[] {new Quote(30, 46.935m, "Broker 1"), new Quote(100, 155.04m, "Broker 2")}),
                new Order(Direction.Sell, "Client B", new[] {new Quote(60, 93.48m, "Broker 2")})
            };

            _service.Buy("Client A", 10);
            _service.Buy("Client B", 40);
            _service.Buy("Client A", 50);
            _service.Buy("Client B", 100);
            _service.Sell("Client B", 80);
            _service.Sell("Client C", 70);
            _service.Buy("Client A", 130);
            _service.Sell("Client B", 60);

            TestUtils.CompareIEnumerable(_service.Orders, transactions,
                (x, y) =>
                    x.ClientId == y.ClientId && x.TotalPrice == y.TotalPrice && x.TotalVolume == y.TotalVolume &&
                    TestUtils.AreEqual(x.Quotes, y.Quotes,
                        (a, b) => a.BrokerId == b.BrokerId && a.LotSize == b.LotSize && a.Price == b.Price));

            Dictionary<string, decimal> clientNetMap = new Dictionary<string, decimal>
            {
                {"Client A", 296.156m},
                {"Client B", 0},
                {"Client C", -109.06m}
            };

            CollectionAssert.AreEqual(clientNetMap, _service.Clients.ToDictionary(c => c.UserId, c => _service.GetClientNetPosition(c.UserId)));

            Dictionary<string, int> brokerVolumeMap = new Dictionary<string, int>
            {
                { "Broker 1", 80},
                { "Broker 2", 460},
            };
            
            CollectionAssert.AreEqual(brokerVolumeMap, _service.Brokers.ToDictionary(b => b.UserId, b => b.VolumeTraded));
        }
    }
}
