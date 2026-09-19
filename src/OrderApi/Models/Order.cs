namespace OrderApi.Models
{
    // An order placed by a customer. Each attribute is stored in a private
    // backing field and exposed through a public property (unit class format).
    public class Order
    {
        private int _Id;
        private string _CustomerName;
        private List<OrderItem> _Items;
        private decimal _TotalAmount;
        private string _Status;
        private DateTime _CreatedAt;

        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        public string CustomerName
        {
            get { return _CustomerName; }
            set { _CustomerName = value; }
        }

        public List<OrderItem> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        public decimal TotalAmount
        {
            get { return _TotalAmount; }
            set { _TotalAmount = value; }
        }

        public string Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        public DateTime CreatedAt
        {
            get { return _CreatedAt; }
            set { _CreatedAt = value; }
        }

        // Default constructor - required by the Web API for JSON model binding.
        public Order()
        {
            _Id = 0;
            _CustomerName = string.Empty;
            _Items = new List<OrderItem>();
            _TotalAmount = 0m;
            _Status = string.Empty;
            _CreatedAt = DateTime.MinValue;
        }

        // Convenience constructor for creating an order in code.
        public Order(string customerName, List<OrderItem> items)
        {
            _Id = 0;
            _CustomerName = customerName;
            _Items = items;
            _TotalAmount = 0m;
            _Status = string.Empty;
            _CreatedAt = DateTime.MinValue;
        }

        public override string ToString()
        {
            int itemCount = _Items.Count;
            return "Order #" + _Id + " for " + _CustomerName
                 + " (" + itemCount + " items, total " + _TotalAmount
                 + ", status " + _Status + ")";
        }
    }
}