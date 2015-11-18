using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DigicoinService;
using DigicoinService.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DigicoinService;

namespace DigicoinTest
{
    [TestClass]
    public class CustomDigiconTests
    {
        private IDigicoinService _service;
        private IList<string> _clientIds;
        
        [TestInitialize]
        public void Init()
        {
            _service = new DigicoinService.DigicoinService();
            _clientIds = Enumerable.Range(1, 100).Select(o => string.Format("Client {0}", o)).ToList();

            foreach (var clientId in _clientIds)
            {
                _service.AddClient(clientId);
            }
            
            Dictionary<int, decimal> commisionMap = new Dictionary<int, decimal>();
            for (int i = 10; i <= 100; i += 10)
            {
                commisionMap.Add(i, 0.05m);
            }
            _service.AddBroker("Broker 1", commisionMap, 1.49m);

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
            _service.AddBroker("Broker 2", commisionMap, 1.52m);
        }

        [TestMethod]
        public void ReturnsExpectedZeroVolume()
        {
            string clientName = "Zero Volume";
            _service.AddClient(clientName);

            IEnumerable<Order> transactions = new List<Order>
            {
                new Order(Direction.Buy, clientName, new[] { new Quote(10, 15.645m, "Broker 1"), new Quote(100, 155.040m, "Broker 2")}),
                new Order(Direction.Sell, clientName, new[] {new Quote(50, 77.900m, "Broker 2")}),
                new Order(Direction.Sell, clientName, new[] {new Quote(60, 93.480m, "Broker 2")})
            };

            _service.Buy(clientName, 110);
            _service.Sell(clientName, 50);
            _service.Sell(clientName, 60);

            TestUtils.CompareOrders(_service.Orders, transactions);

            Assert.AreEqual(0m, _service.GetClientNetPosition(clientName));

            Dictionary<string, int> brokerVolumeMap = new Dictionary<string, int>
            {
                { "Broker 1", 10},
                { "Broker 2", 210},
            };

            CollectionAssert.AreEqual(brokerVolumeMap, _service.Brokers.ToDictionary(b => b.UserId, b => b.VolumeTraded));
        }

        [TestMethod]
        public void ReturnsExpectedPositiveVolume()
        {
            string clientName = "Positive Volume";
            _service.AddClient(clientName);

            IEnumerable<Order> transactions = new List<Order>
            {
                new Order(Direction.Buy, clientName, new[] {new Quote(100, 155.040m, "Broker 2"), new Quote(100, 156.450m, "Broker 1")}),
                new Order(Direction.Sell, clientName, new[] {new Quote(50, 77.900m, "Broker 2")}),
                new Order(Direction.Sell, clientName, new[] {new Quote(50, 77.900m, "Broker 2")}),
            };

            _service.Buy(clientName, 200);
            _service.Sell(clientName, 50);
            _service.Sell(clientName, 50);

            TestUtils.CompareOrders(_service.Orders, transactions);

            Assert.AreEqual(155.782m, _service.GetClientNetPosition(clientName));

            Dictionary<string, int> brokerVolumeMap = new Dictionary<string, int>
            {
                { "Broker 1", 100},
                { "Broker 2", 200},
            };

            CollectionAssert.AreEqual(brokerVolumeMap, _service.Brokers.ToDictionary(b => b.UserId, b => b.VolumeTraded));
        }

        [TestMethod]
        public void ReturnsExpectedNegativeVolume()
        {
            string clientName = "Negative Volume";
            _service.AddClient(clientName);

            IEnumerable<Order> transactions = new List<Order>
            {
                new Order(Direction.Buy, clientName, new[] { new Quote(20, 31.290m, "Broker 1"), new Quote(100, 155.040m, "Broker 2")}),
                new Order(Direction.Sell, clientName, new[] {new Quote(10, 15.645m, "Broker 1"), new Quote(100, 155.040m, "Broker 2")}),
                new Order(Direction.Sell, clientName, new[] {new Quote(30, 46.935m, "Broker 1")}),
            };

            _service.Buy(clientName, 120);
            _service.Sell(clientName, 110);
            _service.Sell(clientName, 30);

            TestUtils.CompareOrders(_service.Orders, transactions);

            Assert.AreEqual(-31.126m, _service.GetClientNetPosition(clientName));

            Dictionary<string, int> brokerVolumeMap = new Dictionary<string, int>
            {
                { "Broker 1", 60},
                { "Broker 2", 200},
            };

            CollectionAssert.AreEqual(brokerVolumeMap, _service.Brokers.ToDictionary(b => b.UserId, b => b.VolumeTraded));
        }

        [TestMethod]
        public void UserDoesntExistsThrowsException()
        {
            string userName = "UserDoesntExistsThrowsException";
            _service.AddClient(userName);
            _service.RemoveClient(userName);
            AssertException.Throws<ArgumentException>(() => _service.RemoveClient(userName));
            AssertException.Throws<ArgumentException>(() => _service.RemoveClient(null));
            AssertException.Throws<ArgumentException>(() => _service.RemoveClient(string.Empty));

            _service.AddBroker(userName, TestUtils.GetDummyCommisionMap(), 10);
            _service.RemoveBroker(userName);
            AssertException.Throws<ArgumentException>(() => _service.RemoveBroker(userName));
            AssertException.Throws<ArgumentException>(() => _service.RemoveBroker(null));
            AssertException.Throws<ArgumentException>(() => _service.RemoveBroker(string.Empty));
        }

        [TestMethod]
        public void UserAlreadyExistsThrowsException()
        {
            string userName = "UserAlreadyExistsThrowsException";
            _service.AddClient(userName);
            AssertException.Throws<ArgumentException>(() => _service.AddClient(userName));

            _service.AddBroker(userName, TestUtils.GetDummyCommisionMap(), 10);
            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, TestUtils.GetDummyCommisionMap(), 10));
        }

        [TestMethod]
        public void ClientCreationBadNameThrowsException()
        {
            AssertException.Throws<ArgumentException>(() => _service.AddClient(null));
            AssertException.Throws<ArgumentException>(() => _service.AddClient(string.Empty));
        }

        [TestMethod]
        public void BrokerCreationBadNameThrowsException()
        {
            AssertException.Throws<ArgumentException>(
                () => _service.AddBroker(null, TestUtils.GetDummyCommisionMap(), 10));
            AssertException.Throws<ArgumentException>(
                () => _service.AddBroker(string.Empty, TestUtils.GetDummyCommisionMap(), 10));
        }

        [TestMethod]
        public void BrokerCreationBadCommissionMapThrowsException()
        {
            string userName = "BrokerCreationBadCommissionMapThrowsException";

            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, null, 10));

            AssertException.Throws<ArgumentException>(
                () => _service.AddBroker(userName, new Dictionary<int, decimal>(), 10));

            IDictionary<int, decimal> commisionMap = TestUtils.GetIncompleteCommissionMap();
            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, commisionMap, 10));
            commisionMap.Add(101, 10);
            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, commisionMap, 10));
            commisionMap = TestUtils.GetIncompleteCommissionMap();
            commisionMap.Add(100, 0);
            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, commisionMap, 10));
            commisionMap = TestUtils.GetIncompleteCommissionMap();
            commisionMap.Add(100, -1);
            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, commisionMap, 10));
            commisionMap = TestUtils.GetIncompleteCommissionMap();
            commisionMap.Add(99, -1);
            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, commisionMap, 10));
            commisionMap = TestUtils.GetIncompleteCommissionMap();
            commisionMap.Add(100, 1);
            commisionMap.Add(110, 1);
            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, commisionMap, 10));
        }

        [TestMethod]
        public void BrokerCreationBadPriceThrowsException()
        {
            string userName = "BrokerCreationBadPriceThrowsException";

            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, TestUtils.GetDummyCommisionMap(), 0));
            AssertException.Throws<ArgumentException>(() => _service.AddBroker(userName, TestUtils.GetDummyCommisionMap(), -1));
        }

        [TestMethod]
        public void ServiceCreationBadArgumentThrowsException()
        {
            AssertException.Throws<ArgumentNullException>(() => new DigicoinService.DigicoinService(new[] {new Client("ServiceCreationBadArgumentThrowsException") }, null));
            AssertException.Throws<ArgumentNullException>(
                () =>
                    new DigicoinService.DigicoinService(null,
                        new[]
                        {new Broker("ServiceCreationBadArgumentThrowsException", TestUtils.GetDummyCommisionMap(), 10)}));
        }

        [TestMethod]
        public void GetClientNetPositionBadArgumentThrowsException()
        {
            //to do
            Assert.Fail();
        }

        [TestMethod]
        public void CustomTests()
        {
            /*for (int i = 10; i < 210; i+=10)
            {
                _service.Buy("Client 1", i);
            }

            var orderMap = _service.Orders.ToDictionary(o => o.TotalVolume, o => o.Quotes.Count());

            IList<Order> transactions = new List<Order>();

            for (int i = 0; i < _clientIds.Count(); i++)
            {
                if (i%2 == 0)
                {
                    _service.Buy(_clientIds[i], 200);
                    transactions.Add(new Order(Direction.Buy, _clientIds[i], new[] {new Quote(100, 15.6450m, "Broker 1")}));
                }
                else
                {
                    _service.Sell(_clientIds[i], 200);
                    transactions.Add(new Order(Direction.Sell, _clientIds[i], new[] { new Quote(10, 15.6450m, "Broker 1") }));
                }
            }

            Dictionary<string, decimal> clientNets = new Dictionary<string, decimal>
            {
                {"Client A", 296.156m},
                {"Client B", 0},
                {"Client C", -109.06m}
            };

            foreach (Client client in _service.Clients)
            {
                var testVal = clientNets[client.UserId];
                //Assert.AreEqual(client.NetPositions, testVal);
            }

            Dictionary<string, int> brokerMap = new Dictionary<string, int>
            {
                { "Broker 1", 80},
                { "Broker 2", 460},
            };

            CollectionAssert.AreEqual(new[] {80, 460}, _service.Brokers.Select(b => b.VolumeTraded).ToArray());*/
        }
    }
}
