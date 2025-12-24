using Ethiopia.Application.Interfaces;
using Ethiopia.Domain.Entities;
using Ethiopia.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Ethiopia.Application.Services
{
    public class ProductService(
        IProductRepository productRepository,
        ILogger<ProductService> logger) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        private readonly ILogger<ProductService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Product> CreateProductAsync(
            string name,
            string sku,
            string description,
            Money price,
            string? category,
            string? brand,
            int initialStock = 0,
            CancellationToken cancellationToken = default)
        {
            ValidateCreateProductParameters(name, sku, price, initialStock);

            // Check if SKU already exists
            if (await _productRepository.SkuExistsAsync(sku, cancellationToken))
            {
                throw new InvalidOperationException($"Product with SKU '{sku}' already exists.");
            }

            var product = new Product(name, sku, description, price, category, brand, initialStock);

            await _productRepository.AddAsync(product, cancellationToken);

            _logger.LogInformation("Product created: {Sku} - {Name}", sku, name);

            return product;
        }

        public async Task UpdateProductPriceAsync(
            Guid productId,
            Money newPrice,
            CancellationToken cancellationToken = default)
        {
            if (newPrice == null) throw new ArgumentNullException(nameof(newPrice));

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product with ID {productId} not found");

            product.UpdatePrice(newPrice);
            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Price updated for product {ProductId}: {NewPrice}",
                productId, newPrice);
        }

        public async Task<StockStatus> CheckStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product with ID {productId} not found");

            return new StockStatus(
                ProductId: productId,
                RequestedQuantity: quantity,
                AvailableQuantity: product.StockQuantity,
                IsAvailable: product.HasSufficientStock(quantity),
                IsLowStock: product.StockQuantity <= 10
            );
        }

        public async Task<bool> UpdateStockAsync(
            Guid productId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                return false;

            if (quantity > 0)
            {
                product.IncreaseStock(quantity);
            }
            else if (quantity < 0)
            {
                product.ReduceStock(Math.Abs(quantity));
            }

            await _productRepository.UpdateAsync(product, cancellationToken);
            return true;
        }

        public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(
            int threshold = 10,
            CancellationToken cancellationToken = default)
        {
            if (threshold < 0)
                throw new ArgumentException("Threshold cannot be negative", nameof(threshold));

            return await _productRepository.GetLowStockProductsAsync(threshold, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> SearchProductsAsync(
            string searchTerm,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            ValidatePaginationParameters(page, pageSize);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _productRepository.GetAllAsync(cancellationToken);
            }

            return await _productRepository.SearchAsync(searchTerm, page, pageSize, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than zero", nameof(count));

            return await _productRepository.GetFeaturedProductsAsync(count, cancellationToken);
        }

        private void ValidateCreateProductParameters(
            string name,
            string sku,
            Money price,
            int initialStock)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required", nameof(name));

            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU is required", nameof(sku));

            if (price == null)
                throw new ArgumentNullException(nameof(price));

            if (initialStock < 0)
                throw new ArgumentException("Initial stock cannot be negative", nameof(initialStock));
        }

        private void ValidatePaginationParameters(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentException("Page must be greater than or equal to 1", nameof(page));

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }

        public async Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _productRepository.GetAllAsync(cancellationToken);
        }




        public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _productRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(
            string category,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            ValidatePaginationParameters(page, pageSize);

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category is required", nameof(category));

            return await _productRepository.GetProductsByCategoryAsync(category, page, pageSize, cancellationToken);
        }

        public async Task<bool> DeleteProductAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return false;

            product.Delete(deletedBy);
            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Product deleted: {Id} by {DeletedBy}", id, deletedBy);
            return true;
        }

        public async Task<Product?> RestoreProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return null;

            product.Restore();
            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Product restored: {Id}", id);
            return product;
        }

        public async Task<Product?> ActivateProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return null;

            product.IsActive = true;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Product activated: {Id}", id);
            return product;
        }

        public async Task<Product?> DeactivateProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return null;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Product deactivated: {Id}", id);
            return product;
        }

        public async Task<int> GetProductCountAsync(CancellationToken cancellationToken = default)
        {
            return await _productRepository.GetTotalCountAsync(cancellationToken);
        }
    }
}