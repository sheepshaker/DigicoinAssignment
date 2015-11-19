
namespace DigicoinService.Model
{
    public class Quote
    {
        private Quote()
        {
            //dummy quote for the cartesian product calculation only
            IsEmpty = true;
        }

        public Quote(int lotSize, decimal priceAfterCommission, string brokerId)
        {
            PriceAfterCommission = priceAfterCommission;
            LotSize = lotSize;
            BrokerId = brokerId;
        }

        public decimal PriceAfterCommission { get; private set; }
        public int LotSize { get; private set; }
        public string BrokerId { get; private set; }
        internal bool IsEmpty { get; private set; }

        internal static Quote Empty
        {
            get { return new Quote(); }
        }
    }
}
