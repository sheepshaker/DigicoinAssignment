
namespace DigicoinService.Model
{
    public class Quote
    {
        private Quote()
        {
            //dummy quote for the cartesian product calculation only
            IsEmpty = true;
        }

        internal Quote(int lotSize, decimal price, Broker broker)
        {
            Price = price;
            LotSize = lotSize;
            Broker = broker;
        }

        internal decimal Price { get; private set; }
        internal int LotSize { get; private set; }
        internal Broker Broker { get; private set; }
        internal bool IsEmpty { get; private set; }

        internal static Quote Empty
        {
            get
            {
                return new Quote();
            }
        }
    }
}
