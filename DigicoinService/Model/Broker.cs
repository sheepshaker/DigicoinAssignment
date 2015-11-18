using System;
using System.Collections.Generic;
using System.Linq;

namespace DigicoinService.Model
{
    public class Broker : UserBase
    {
        private readonly IEnumerable<Quote> _quotes;
        private int _volumeTraded;

        public Broker(string brokerId, IDictionary<int, decimal> commissionMap, decimal price) : base(brokerId)
        {
            if (commissionMap == null)
            {
                throw new ArgumentNullException("commissionMap");
            }

            Price = price;
            _quotes = CalculateQuotes(commissionMap);
        }

        private IEnumerable<Quote> CalculateQuotes(IDictionary<int, decimal> commissionMap)
        {
            var quotes = new List<Quote>();

            //add dummy quote
            quotes.Add(Quote.Empty);

            //pre-calculate quotes
            foreach (var key in commissionMap.Keys)
            {
                Quote quote = new Quote(key, GetPriceAfterCommission(key, commissionMap), this);
                quotes.Add(quote);
            }

            return quotes;
        } 

        private decimal GetPriceAfterCommission(int lotSize, IDictionary<int, decimal> commissionMap)
        {
            decimal commission;

            if (commissionMap.TryGetValue(lotSize, out commission) == false)
            {
                throw new Exception("Invalid lot size");
            }

            var quotePrice = lotSize * Price;
           
            return quotePrice + quotePrice * commission;
        }

        internal IEnumerable<Quote> GetQuotes(int lotSize)
        {
            return _quotes.Where(q => q.LotSize <= lotSize);
        }

        public int VolumeTraded
        {
            get
            {
                return _volumeTraded;
            }
        }

        internal void AddVolume(int volume)
        {
            _volumeTraded += volume;
        }

        internal decimal Price { get; set; }
    }
}
