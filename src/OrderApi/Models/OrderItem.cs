namespace OrderApi.Models
{
   
    public class OrderItem
    {
        private string _ProductName;
        private int _Quantity;
        private decimal _UnitPrice;

        public string ProductName
        {
            get { return _ProductName; }
            set { _ProductName = value; }
        }

        public int Quantity
        {
            get { return _Quantity; }
            set { _Quantity = value; }
        }

        public decimal UnitPrice
        {
            get { return _UnitPrice; }
            set { _UnitPrice = value; }
        }

        public OrderItem()
        {
            _ProductName = string.Empty;
            _Quantity = 0;
            _UnitPrice = 0m;
        }

        public OrderItem(string productName, int quantity, decimal unitPrice)
        {
            _ProductName = productName;
            _Quantity = quantity;
            _UnitPrice = unitPrice;
        }

        public override string ToString()
        {
            return _Quantity + " x " + _ProductName + " @ " + _UnitPrice;
        }
    }
}