using Mango.Service.OrderApi.Models;

namespace Mango.Service.OrderApi.Repository;

public interface IOrderRepository
{
    Task<bool> AddOrder(OrderHeader orderHeader);
    Task UpdateOrderPaymentStatus(int orderHeaderId, bool paid);
}