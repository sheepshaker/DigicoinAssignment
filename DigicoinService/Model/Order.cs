using System;

namespace DigicoinService.Model
{
    public class Order
    {
        public Order(Direction direction, int lotSize)
        {
            if (lotSize < 10)
            {
                throw new ArgumentException("Invalid size: 10 is minimum", "lotSize");
            }

            if (lotSize % 10 > 0)
            {
                throw new ArgumentException("Invalid size: only multiples of 10", "lotSize");
            }

            if (lotSize > 200)
            {
                throw new ArgumentException("Invalid size: no more than 100 per Broker", "lotSize");
            }

            Direction = direction;
            LotSize = lotSize;
        }

        public int LotSize { get; private set; }
        public Direction Direction { get; private set; }
    }
}
