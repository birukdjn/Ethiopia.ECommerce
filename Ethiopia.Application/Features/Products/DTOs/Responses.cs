using Ethiopia.Domain.Entities;


namespace Ethiopia.Application.Features.Products.DTOs
{
    // Additional DTOs
    public record SearchResponse(
        List<ProductResponse> Products,
        int Page,
        int PageSize,
        int TotalCount
    );

    public record UpdateStockRequest(
        int Quantity
    );

    public record UpdateStockResponse(
        Guid ProductId,
        int QuantityChanged,
        string Message
    );

    // Existing DTOs
    public record CreateProductRequest(
        string Name,
        string Sku,
        decimal Price,
        string Currency,
        string? Description,
        string? Category,
        string? Brand,
        int InitialStock = 0
    );

    public record UpdatePriceRequest(
        decimal NewPrice,
        string Currency
    );

    public record ProductResponse(
        Guid Id,
        string Name,
        string Sku,
        string? Description,
        decimal Price,
        string Currency,
        int StockQuantity,
        string? Category,
        string? Brand,
        bool IsActive,
        DateTime CreatedAt
    );



    public record StockStatusResponse(
        Guid ProductId,
        int RequestedQuantity,
        int AvailableQuantity,
        bool IsAvailable,
        bool IsLowStock
    );

    public record ErrorResponse(string Error, string Message);


}