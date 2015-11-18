using System;
using System.Collections.Generic;
using System.Linq;

namespace DigicoinService.Model
{
    public class Client : UserBase
    {
        private readonly IList<Order> _orders;

        public Client(string clientId) : base(clientId)
        {
            _orders = new List<Order>();
        }

        public decimal NetPositions
        {
            get
            {
                decimal netPositions = 0;
                if (_orders.Any())
                {
                    netPositions = _orders.Average(o => o.TotalPrice/Math.Abs(o.TotalVolume))*
                                   _orders.Sum(o => o.TotalVolume);

                    //test output seem to be 3 DP max, e.g.: 296.156 as opposed to 296.1564103
                    netPositions = Math.Round(netPositions, 3);
                }

                return netPositions;
            }
        }

        internal void AddOrder(Order order)
        {
            _orders.Add(order);
        }
    }
}
