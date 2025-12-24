using Ethiopia.Application.Interfaces;
using Ethiopia.Domain.Entities;
using Ethiopia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ethiopia.Application.Services;

namespace Ethiopia.Infrastructure.Data.Repositories
{
    public class ProductRepository(AppDbContext context, ILogger<ProductRepository> logger) : IProductRepository
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<ProductRepository> _logger = logger;

        // Interface implementation
        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        }

        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Sku == sku && !p.IsDeleted, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(
            string category,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.Category == category && !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(
            string searchTerm,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p =>
                    (p.Name.Contains(searchTerm) ||
                     p.Description != null && p.Description.Contains(searchTerm) ||
                     p.Brand != null && p.Brand.Contains(searchTerm)) &&
                    !p.IsDeleted &&
                    p.IsActive)
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderByDescending(p => p.AverageRating)
                .ThenByDescending(p => p.ReviewCount)
                .Take(count)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Guid> AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Product added: {product.Sku} - {product.Name}");
            return product.Id;
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Product updated: {product.Sku} - {product.Name}");
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(id, cancellationToken);
            if (product != null)
            {
                product.Delete("System"); // Using the domain method
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product soft deleted: {Id}", id);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        }

        public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AnyAsync(p => p.Sku == sku && !p.IsDeleted, cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .CountAsync(p => !p.IsDeleted && p.IsActive, cancellationToken);
        }

        public async Task<int> GetCategoryCountAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .CountAsync(p => p.Category == category && !p.IsDeleted && p.IsActive, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(
            int threshold = 10,
            CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.StockQuantity <= threshold && !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.StockQuantity)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null) return false;

            product.StockQuantity += quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stock updated for product {ProductId}: {Quantity}", productId, quantity);
            return true;
        }

        // Helper method for your original CheckStockAsync
        public async Task<StockStatus> CheckStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product with ID {productId} not found");

            return new StockStatus(
                ProductId: productId,
                RequestedQuantity: quantity,
                AvailableQuantity: product.StockQuantity,
                IsAvailable: product.HasSufficientStock(quantity),
                IsLowStock: product.StockQuantity <= 10
            );
        }
    }


}