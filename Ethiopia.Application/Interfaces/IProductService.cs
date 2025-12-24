using Ethiopia.Application.Services;
using Ethiopia.Domain.Entities;
using Ethiopia.Domain.ValueObjects;

namespace Ethiopia.Application.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(
            string name,
            string sku,
            string description,
            Money price,
            string? category,
            string? brand,
            int initialStock = 0,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(
            string category,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
        Task UpdateProductPriceAsync(
            Guid productId,
            Money newPrice,
            CancellationToken cancellationToken = default);
        Task<StockStatus> CheckStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default);
        Task<bool> UpdateStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> GetLowStockProductsAsync(
            int threshold = 10,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> SearchProductsAsync(
            string searchTerm,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(
            int count = 10,
            CancellationToken cancellationToken = default);
        Task<bool> DeleteProductAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
        Task<Product?> RestoreProductAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Product?> ActivateProductAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Product?> DeactivateProductAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetProductCountAsync(CancellationToken cancellationToken = default);
    }
}
