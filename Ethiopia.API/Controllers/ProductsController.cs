using Ethiopia.Application.Interfaces;
using Ethiopia.Domain.Entities;
using Ethiopia.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Ethiopia.Application.Features.Products.DTOs;

namespace Ethiopia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var price = new Money(request.Price, request.Currency);
            var product = await _productService.CreateProductAsync(
                request.Name,
                request.Sku,
                request.Description ?? string.Empty,
                price,
                request.Category,
                request.Brand,
                request.InitialStock,
                cancellationToken);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToProductResponse(product));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new ErrorResponse("Duplicate SKU", ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("Invalid input", ex.Message));
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound(new ErrorResponse("Product not found", $"Product with ID {id} not found"));
        return Ok(MapToProductResponse(product));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts(CancellationToken cancellationToken = default)
    {
        var products = await _productService.GetAllProductsAsync(cancellationToken);
        return Ok(products.Select(MapToProductResponse));
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResponse>> SearchProducts(
        [FromQuery] string? term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _productService.SearchProductsAsync(term ?? "", page, pageSize, cancellationToken);
            return Ok(new SearchResponse([.. products.Select(MapToProductResponse)], page, pageSize, products.Count));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("Invalid search parameters", ex.Message));
        }
    }

    [HttpGet("category/{category}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchResponse>> GetProductsByCategory(
        string category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var products = await _productService.GetProductsByCategoryAsync(category, page, pageSize, cancellationToken);
        return Ok(new SearchResponse(products.Select(MapToProductResponse).ToList(), page, pageSize, products.Count));
    }

    [HttpGet("featured")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetFeaturedProducts(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var products = await _productService.GetFeaturedProductsAsync(count, cancellationToken);
        return Ok(products.Select(MapToProductResponse));
    }

    [HttpPut("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var newPrice = new Money(request.NewPrice, request.Currency);
            await _productService.UpdateProductPriceAsync(id, newPrice, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse("Product not found", ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse("Invalid price", ex.Message));
        }
    }

    [HttpPut("{id:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateStockResponse>> UpdateStock(Guid id, [FromBody] UpdateStockRequest request, CancellationToken cancellationToken = default)
    {
        var success = await _productService.UpdateStockAsync(id, request.Quantity, cancellationToken);
        if (!success) return NotFound(new ErrorResponse("Product not found", $"Product with ID {id} not found"));
        return Ok(new UpdateStockResponse(id, request.Quantity, "Stock updated successfully"));
    }

    [HttpGet("{id:guid}/stock-status")]
    public async Task<ActionResult<StockStatusResponse>> CheckStock(Guid id, [FromQuery] int quantity = 1, CancellationToken cancellationToken = default)
    {
        var status = await _productService.CheckStockAsync(id, quantity, cancellationToken);
        return Ok(new StockStatusResponse(status.ProductId, status.RequestedQuantity, status.AvailableQuantity, status.IsAvailable, status.IsLowStock));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetLowStockProducts([FromQuery] int threshold = 10, CancellationToken cancellationToken = default)
    {
        var products = await _productService.GetLowStockProductsAsync(threshold, cancellationToken);
        return Ok(products.Select(MapToProductResponse));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var success = await _productService.DeleteProductAsync(id,deletedBy:"system", cancellationToken);
        if (!success) return NotFound(new ErrorResponse("Product not found", $"Product with ID {id} not found"));
        return NoContent();
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult<ProductResponse>> RestoreProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.RestoreProductAsync(id, cancellationToken);
        if (product == null) return NotFound(new ErrorResponse("Product not found", $"Product with ID {id} not found"));
        return Ok(MapToProductResponse(product));
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<ActionResult<ProductResponse>> ActivateProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.ActivateProductAsync(id, cancellationToken);
        if (product == null) return NotFound(new ErrorResponse("Product not found", $"Product with ID {id} not found"));
        return Ok(MapToProductResponse(product));
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult<ProductResponse>> DeactivateProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.DeactivateProductAsync(id, cancellationToken);
        if (product == null) return NotFound(new ErrorResponse("Product not found", $"Product with ID {id} not found"));
        return Ok(MapToProductResponse(product));
    }
    private ProductResponse MapToProductResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Description,
            product.Price,
            product.Currency,
            product.StockQuantity,
            product.Category,
            product.Brand,
            product.IsActive,
            product.CreatedAt);
    }
}
