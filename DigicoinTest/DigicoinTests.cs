using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DigicoinService;
using DigicoinService.Model;

namespace DigicoinTest
{
    [TestClass]
    public class DefaultDigiconTests
    {
        private IDigicoinService _service = new DigicoinService.DigicoinService();
        private readonly Dictionary<string, int> _brokerVolumeMap = new Dictionary<string, int>();


        [TestMethod]
        public void DefaultTests()
        {
            decimal actualPrice;
            actualPrice =_service.ExecuteOrder("Client A", new Order(Direction.Buy, 10));
            Assert.AreEqual(actualPrice, 15.6450m);
            actualPrice =_service.ExecuteOrder("Client B", new Order(Direction.Buy, 40));
            Assert.AreEqual(actualPrice, 62.58m);
            actualPrice = _service.ExecuteOrder("Client A", new Order(Direction.Buy, 50));
            Assert.AreEqual(actualPrice, 77.9m);
            actualPrice = _service.ExecuteOrder("Client B", new Order(Direction.Buy, 100));
            Assert.AreEqual(actualPrice, 155.04m);
            actualPrice = _service.ExecuteOrder("Client B", new Order(Direction.Sell, 80));
            Assert.AreEqual(actualPrice, 124.64m);
            actualPrice = _service.ExecuteOrder("Client C", new Order(Direction.Sell, 70));
            Assert.AreEqual(actualPrice, 109.06m);
            actualPrice = _service.ExecuteOrder("Client A", new Order(Direction.Buy, 130));
            Assert.AreEqual(actualPrice, 201.975m);
            actualPrice = _service.ExecuteOrder("Client B", new Order(Direction.Sell, 60));
            Assert.AreEqual(actualPrice, 93.48m);

            Dictionary<string, decimal> clientNetMap = new Dictionary<string, decimal>
            {
                {"Client A", 296.156m},
                {"Client B", 0},
                {"Client C", -109.06m}
            };

            CollectionAssert.AreEqual(clientNetMap, _service.ClientsNetPosition);

            AddVolume("Broker 1", 80);
            AddVolume("Broker 2", 460);

            CollectionAssert.AreEqual(_brokerVolumeMap, _service.BrokersVolumeTraded);
        }

        [TestMethod]
        public void ReturnsExpectedZeroPosition()
        {
            string clientName = "Zero Position";

            decimal actualPrice;
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Buy, 110));
            Assert.AreEqual(15.645m + 155.040m, actualPrice);
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Sell, 50));
            Assert.AreEqual(77.900m, actualPrice);
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Sell, 60));
            Assert.AreEqual(93.480m, actualPrice);


            Assert.AreEqual(0m, _service.ClientsNetPosition[clientName]);

            AddVolume("Broker 1", 10);
            AddVolume("Broker 2", 210);

            CollectionAssert.AreEqual(_brokerVolumeMap, _service.BrokersVolumeTraded);
        }

        [TestMethod]
        public void ReturnsExpectedPositivePosition()
        {
            string clientName = "Positive Position";

            decimal actualPrice;
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Buy, 200));
            Assert.AreEqual(155.040m + 156.450m, actualPrice);
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Sell, 50));
            Assert.AreEqual(77.900m, actualPrice);
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Sell, 50));
            Assert.AreEqual(77.900m, actualPrice);

            Assert.AreEqual(155.782m, _service.ClientsNetPosition[clientName]);

            AddVolume("Broker 1", 100);
            AddVolume("Broker 2", 200);

            CollectionAssert.AreEqual(_brokerVolumeMap, _service.BrokersVolumeTraded);
        }

        [TestMethod]
        public void ReturnsExpectedNegativePosition()
        {
            string clientName = "Negative Position";

            decimal actualPrice;
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Buy, 120));
            Assert.AreEqual(31.290m + 155.040m, actualPrice);
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Sell, 110));
            Assert.AreEqual(15.645m + 155.040m, actualPrice);
            actualPrice = _service.ExecuteOrder(clientName, new Order(Direction.Sell, 30));
            Assert.AreEqual(46.935m, actualPrice);

            Assert.AreEqual(-31.126m, _service.ClientsNetPosition[clientName]);

            AddVolume("Broker 1", 60);
            AddVolume("Broker 2", 200);

            CollectionAssert.AreEqual(_brokerVolumeMap, _service.BrokersVolumeTraded);
        }

        [TestMethod]
        public void RejectedOrders()
        {
            //i.e. size over 200 or less than 0 or bad increment
            AssertException.Throws<ArgumentException>(() => { new Order(Direction.Buy, 210); });
            AssertException.Throws<ArgumentException>(() => { new Order(Direction.Buy, -10); });
            AssertException.Throws<ArgumentException>(() => { new Order(Direction.Buy, -1); });
            AssertException.Throws<ArgumentException>(() => { new Order(Direction.Buy, 0); });
            AssertException.Throws<ArgumentException>(() => { new Order(Direction.Buy, 1); });
            AssertException.Throws<ArgumentException>(() => { new Order(Direction.Buy, 9); });
            AssertException.Throws<ArgumentException>(() => { new Order(Direction.Buy, 199); });
            AssertException.Throws<ArgumentException>(() => { new Order(Direction.Buy, 201); });

            var order = new Order(Direction.Buy, 10);
            AssertException.Throws<ArgumentException>(() => { _service.ExecuteOrder(string.Empty, order); });
            AssertException.Throws<ArgumentException>(() => { _service.ExecuteOrder(null, order); });
        }

        private void AddVolume(string brokerId, int lotSize)
        {
            if (_brokerVolumeMap.ContainsKey(brokerId))
            {
                _brokerVolumeMap[brokerId] += lotSize;
            }
            else
            {
                _brokerVolumeMap.Add(brokerId, lotSize);
            }
        }
    }
}
