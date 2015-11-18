using System.Collections.Generic;
using System.Linq;

namespace DigicoinService.Model
{
    public class Order
    {
        public Order(Direction direction, IEnumerable<Quote> quotes)
        {
            Quotes = quotes;
            var enumerable = Quotes as IList<Quote> ?? Quotes.ToList();
            TotalPrice = enumerable.Sum(q => q.Price);
            TotalVolume = enumerable.Sum(q => direction == Direction.Buy ? q.LotSize : -q.LotSize);
        }

        internal IEnumerable<Quote> Quotes { get; set; }
        public decimal TotalPrice { get; private set; }
        public decimal TotalVolume { get; private set; }
    }
}
