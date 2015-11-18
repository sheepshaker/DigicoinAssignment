using System.Collections.Generic;
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
            var order = _service.Buy("Client A", 10);
            Assert.AreEqual(order.TotalPrice, 15.645m);
            Assert.AreEqual(order.TotalVolume, 10);

            order = _service.Buy("Client B", 40);
            Assert.AreEqual(order.TotalPrice, 62.58m);
            Assert.AreEqual(order.TotalVolume, 40);

            order = _service.Buy("Client A", 50);
            Assert.AreEqual(order.TotalPrice, 77.9m);
            Assert.AreEqual(order.TotalVolume, 50);

            order = _service.Buy("Client B", 100);
            Assert.AreEqual(order.TotalPrice, 155.04m);
            Assert.AreEqual(order.TotalVolume, 100);

            order = _service.Sell("Client B", 80);
            Assert.AreEqual(order.TotalPrice, 124.64m);
            Assert.AreEqual(order.TotalVolume, -80);

            order = _service.Sell("Client C", 70);
            Assert.AreEqual(order.TotalPrice, 109.06m);
            Assert.AreEqual(order.TotalVolume, -70);

            order = _service.Buy("Client A", 130);
            Assert.AreEqual(order.TotalPrice, 201.975m);
            Assert.AreEqual(order.TotalVolume, 130);

            order = _service.Sell("Client B", 60);
            Assert.AreEqual(order.TotalPrice, 93.48m);
            Assert.AreEqual(order.TotalVolume, -60);


            Dictionary<string, decimal> clientNets = new Dictionary<string, decimal>
            {
                {"Client A", 296.156m},
                {"Client B", 0},
                {"Client C", -109.06m}
            };

            foreach (Client client in _service.Clients)
            {
                var testVal = clientNets[client.UserId];
                Assert.AreEqual(client.NetPositions, testVal);
            }

            Dictionary<string, int> brokerMap = new Dictionary<string, int>
            {
                { "Broker 1", 80},
                { "Broker 2", 460},
            };

            foreach (Broker broker in _service.Brokers)
            {
                var testVal = brokerMap[broker.UserId];
                Assert.AreEqual(broker.VolumeTraded, testVal);
            }
        }
    }
}
