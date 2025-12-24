using Ethiopia.API.Middleware;
using Ethiopia.API.Seeders;
using Ethiopia.Application.Features.Products.Queries;
using Ethiopia.Application.Interfaces;
using Ethiopia.Application.Services;
using Ethiopia.Infrastructure.Data;
using Ethiopia.Infrastructure.Data.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;
using Ethiopia.API.Validators;

// ========== variables ==========
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// ========== Configuration ==========
var connectionString = configuration.GetConnectionString("DefaultConnection");

// ========== Add Services ==========
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddResponseCompression();


// ========== Database ==========
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

// ========== CORS ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("ProductionCors", policy =>
    {
        policy.WithOrigins(
                "https://localhost:3000",
                "https://192.168.100.167:3000",
                "https://yourapp.ethiopia.com",
                "https://admin.yourapp.ethiopia.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ========== Authentication & Authorization ==========
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));

    options.AddPolicy("ProductManagement", policy =>
        policy.RequireClaim("permission", "product.write", "product.read"));
});

// ========== MediatR ==========
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetAllProductsQuery).Assembly);
});

// ========== AutoMapper ==========
services.AddAutoMapper(cfg => { }, typeof(GetAllProductsQuery).Assembly);

// ========== Repositories & Services ==========
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IInventoryRepository, InventoryRepository>();
services.AddScoped<IProductService, ProductService>();

// ========== Health Checks ==========
services.AddHealthChecks()
    // App itself (liveness)
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live"])

    // EF Core DbContext (readiness)
    .AddDbContextCheck<AppDbContext>(
        name: "db-context",
        tags: ["ready"])

    // SQL Server connection (readiness)
    .AddSqlServer(
        connectionString,
        name: "sql-server",
        tags: ["ready"]);

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

// ========== Swagger/OpenAPI ==========
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Ethiopian.ECommerse", 
        Version = "v1",
        Description = "eCommerce Platform API for Ethiopian Market",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@ethiopian-ecommerce.com"
        },
        License = new OpenApiLicense
        {
            Name = "Commercial License"
        }
    });

    // ========== XML comments  ==========
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // ========== JWT Bearer Authentication ==========
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme. Example: \'Authorization: Bearer {token}\'",
        Name = "Authorization",
        In = ParameterLocation.Header,


    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement

    {

        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});


// ========== Response Caching ==========
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024;
    options.UseCaseSensitivePaths = false;
});

// ========== Rate Limiting ==========
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});


// ========== Build App ==========
var app = builder.Build();

// ========== Configure Middleware Pipeline ==========

// Development-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAll");

    app.UseSwagger(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ethiopian eCommerce V1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Ethiopian eCommerce API Documentation";
        options.EnablePersistAuthorization();
        options.DisplayRequestDuration();
    });
    // Seed database in development
    //using var scope = app.Services.CreateScope();
    await DatabaseSeeder.SeedAsync(app);
}

else
{
    app.UseExceptionHandler("/error");
    app.UseCors("ProductionCors");
    app.UseHsts();
}

// ========== Production Middleware ==========
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Response Compression
app.UseResponseCompression();

// Response Caching
app.UseResponseCaching();

// Rate Limiting
app.UseRateLimiter();

// Static Files (for product images, etc.)
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = false,
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 day
        ctx.Context.Response.Headers.Append(
            "Cache-Control", "public, max-age=86400");
    }
});

// Routing
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();

// ======== Health Check endpoint============//
// Liveness: is the app running?
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
}).AllowAnonymous();

// Readiness: can app receive traffic?
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

// Full detailed report (monitoring/debug)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = e.Value.Duration.TotalMilliseconds
            }),
            timestamp = DateTime.UtcNow
        });

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
}).AllowAnonymous();


// Global error handling endpoint
app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    var response = new
    {
        message = app.Environment.IsDevelopment()
            ? exception?.Message
            : "An error occurred. Please try again later.",
        requestId = context.TraceIdentifier
    };

    return Results.Json(response, statusCode: context.Response.StatusCode);
});

app.Run();