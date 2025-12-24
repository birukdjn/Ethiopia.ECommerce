using Ethiopia.Domain.Entities;

namespace Ethiopia.Application.Interfaces
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default);
    }
}
