namespace Ethiopia.Application.Services
{
    public record StockStatus(
        Guid ProductId,
        int RequestedQuantity,
        int AvailableQuantity,
        bool IsAvailable,
        bool IsLowStock
    );
}