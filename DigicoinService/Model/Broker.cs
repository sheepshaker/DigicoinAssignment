using System;
using System.Collections.Generic;
using System.Linq;

namespace DigicoinService.Model
{
    public class Broker
    {
        private readonly IEnumerable<Quote> _quotes;
        private int _volumeTraded;
        //public string BrokerId { get; private set; }

        public Broker(string brokerId, IDictionary<int, decimal> commissionMap, decimal price)
        {
            _quotes = CalculateQuotes(commissionMap, price, brokerId);
        }

        private IEnumerable<Quote> CalculateQuotes(IDictionary<int, decimal> commissionMap, decimal price, string brokerId)
        {
            var quotes = new List<Quote>();

            //add dummy quote
            quotes.Add(Quote.Empty);

            //pre-calculate quotes
            foreach (var lotSizeIncrement in commissionMap.Keys)
            {
                Quote quote = new Quote(lotSizeIncrement, GetPriceAfterCommission(lotSizeIncrement, commissionMap, price), brokerId);
                quotes.Add(quote);
            }

            return quotes;
        } 

        private decimal GetPriceAfterCommission(int lotSize, IDictionary<int, decimal> commissionMap, decimal price)
        {
            decimal commission;

            if (commissionMap.TryGetValue(lotSize, out commission) == false)
            {
                throw new Exception("Invalid lot size");
            }

            var quotePrice = lotSize * price;
           
            return Math.Round(quotePrice + quotePrice * commission, 3);
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
    }
}
