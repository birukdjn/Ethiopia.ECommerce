using Ethiopia.Domain.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Ethiopia.Domain.Entities;

public class Inventory : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }

    [Range(0, int.MaxValue)]
    public int AvailableStock { get; private set; }

    [Range(0, int.MaxValue)]
    public int ReservedStock { get; private set; }

    public int ReorderThreshold { get; set; } = 10;
    public int MaxStock { get; set; } = 1000;

    public virtual Product Product { get; set; } = default!;

    public int GetAvailableForSale() => AvailableStock - ReservedStock;

    public void Reserve(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        if (GetAvailableForSale() < quantity)
            throw new InsufficientStockException(quantity, GetAvailableForSale());

        ReservedStock += quantity;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        if (quantity > ReservedStock)
            throw new ArgumentException("Cannot release more than reserved");

        ReservedStock -= quantity;
    }

    public void Fulfill(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        if (quantity > ReservedStock)
            throw new ArgumentException("Cannot fulfill more than reserved");

        AvailableStock -= quantity;
        ReservedStock -= quantity;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        if (AvailableStock + quantity > MaxStock)
            throw new ArgumentException($"Cannot exceed max stock of {MaxStock}");

        AvailableStock += quantity;
    }

    // IAuditableEntity implementation
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}