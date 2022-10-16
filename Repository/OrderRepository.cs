using Mango.Service.OrderApi.Data;
using Mango.Service.OrderApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.OrderApi.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public OrderRepository(DbContextOptions<ApplicationDbContext> options)
    {
        _options = options;
    }
    public async Task<bool> AddOrder(OrderHeader orderHeader)
    {
        await using var _db = new ApplicationDbContext(_options);
        _db.OrderHeaders.Add(orderHeader);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task UpdateOrderPaymentStatus(int orderHeaderId, bool paid)
    {
        await using var _db = new ApplicationDbContext(_options);
        var orderHeaderFromDb = await _db.OrderHeaders.FirstOrDefaultAsync(u => u.OrderHeaderId == orderHeaderId);
        if (orderHeaderFromDb != null)
        {
            orderHeaderFromDb.PaymentStatus = paid;
            await _db.SaveChangesAsync();
        }
    }
}