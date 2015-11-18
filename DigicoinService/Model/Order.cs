using System.Collections.Generic;
using System.Linq;

namespace DigicoinService.Model
{
    public class Order
    {
        public Order(Direction direction, string clientId, IEnumerable<Quote> quotes)
        {
            ClientId = clientId;
            Quotes = quotes;
            var enumerable = Quotes as IList<Quote> ?? Quotes.ToList();
            TotalPrice = enumerable.Sum(q => q.Price);
            TotalVolume = enumerable.Sum(q => direction == Direction.Buy ? q.LotSize : -q.LotSize);
        }

        public string ClientId { get; private set; }
        public IEnumerable<Quote> Quotes { get; private set; }
        public decimal TotalPrice { get; private set; }
        public decimal TotalVolume { get; private set; }
    }
}
