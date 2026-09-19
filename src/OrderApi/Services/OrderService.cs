using System.Collections.Concurrent;
using OrderApi.Models;

namespace OrderApi.Services
{
  
    public class OrderService : IOrderService
    {
        private readonly ConcurrentDictionary<int, Order> _Orders;
        private int _NextId;

        public OrderService()
        {
            _Orders = new ConcurrentDictionary<int, Order>();
            _NextId = 0;
        }

        public IEnumerable<Order> GetAll()
        {
            return _Orders.Values;
        }

        public Order? GetById(int id)
        {
            if (_Orders.TryGetValue(id, out Order? order))
            {
                return order;
            }
            return null;
        }

        public Order Create(Order order)
        {
            int id = Interlocked.Increment(ref _NextId);
            order.Id = id;
            order.CreatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(order.Status))
            {
                order.Status = "Created";
            }

            _Orders[id] = order;
            return order;
        }

        public bool Update(int id, Order order)
        {
            if (_Orders.ContainsKey(id) == false)
            {
                return false;
            }

            order.Id = id;
            _Orders[id] = order;
            return true;
        }

        public bool Delete(int id)
        {
            return _Orders.TryRemove(id, out _);
        }
    }
}