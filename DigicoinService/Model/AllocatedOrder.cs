using System;
using System.Collections.Generic;

namespace DigicoinService.Model
{
    internal class AllocatedOrder
    {
        public AllocatedOrder(Order clientOrder, decimal orderPrice, IEnumerable<Quote> quotes)
        {
            ClientOrder = clientOrder;
            OrderPrice = orderPrice;
            BrokerQuotes = quotes;
        }

        public Order ClientOrder { get; private set; }
        public decimal OrderPrice { get; private set; }
        public IEnumerable<Quote> BrokerQuotes { get; private set; }

        internal int VolumeTraded
        {
            get { return ClientOrder.Direction == Direction.Buy ? ClientOrder.LotSize : -ClientOrder.LotSize; }
        }
    }
}
