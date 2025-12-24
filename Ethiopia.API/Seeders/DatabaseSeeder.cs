using Ethiopia.Domain.Entities;
using Ethiopia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ethiopia.API.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILoggerFactory>()
                                 .CreateLogger("DatabaseSeeder");

        try
        {
            var context = services.GetRequiredService<AppDbContext>();

            // Apply migrations
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            // Seed initial data
            await SeedProductsAsync(context, logger);
            await SeedCategoriesAsync(context, logger);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedProductsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Products.AnyAsync())
        {
            logger.LogInformation("Products already exist. Skipping product seeding.");
            return;
        }

        var products = new[]
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "የኢትዮጵያ ቡና",
                Sku = "ET-COFFEE-001",
                Description = "Premium Ethiopian Coffee Beans - Yirgacheffe Region",
                Price = 450.00m,
                StockQuantity = 100,
                Category = "Coffee",
                Brand = "Ethiopian Origins",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "ሽሚዝ ሸሚዝ",
                Sku = "ET-CLOTHING-001",
                Description = "Traditional Ethiopian Shirt - Handwoven Netela Style",
                Price = 1200.00m,
                StockQuantity = 50,
                Category = "Clothing",
                Brand = "Habesha Fashion",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "ጤፍ ዱቄት",
                Sku = "ET-FOOD-001",
                Description = "Organic Teff Flour - Gluten Free, High in Iron",
                Price = 350.00m,
                StockQuantity = 200,
                Category = "Food",
                Brand = "Ethio Organics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "ሙሽራ ማር",
                Sku = "ET-HONEY-001",
                Description = "Pure Ethiopian Honey - Forest Mushroom Type",
                Price = 600.00m,
                StockQuantity = 75,
                Category = "Food",
                Brand = "Ethiopian Natural",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} products.", products.Length);
    }

    private static async Task SeedCategoriesAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Categories.AnyAsync())
        {
            logger.LogInformation("Categories already exist. Skipping category seeding.");
            return;
        }

        var categories = new[]
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Coffee",
                Description = "Authentic Ethiopian Coffee Beans and Products",
                ImageUrl = "/images/categories/coffee.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Clothing",
                Description = "Traditional Ethiopian Clothing and Fashion",
                ImageUrl = "/images/categories/clothing.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                Description = "Traditional Ethiopian Food Products",
                ImageUrl = "/images/categories/food.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Handicrafts",
                Description = "Ethiopian Handmade Crafts and Art",
                ImageUrl = "/images/categories/handicrafts.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} categories.", categories.Length);
    }   
   
}