using Ethiopia.Application.Interfaces;
using Ethiopia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ethiopia.Infrastructure.Data.Repositories;

public class InventoryRepository(AppDbContext context) : IInventoryRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Inventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);
    }

    public async Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync(cancellationToken);
    }
}