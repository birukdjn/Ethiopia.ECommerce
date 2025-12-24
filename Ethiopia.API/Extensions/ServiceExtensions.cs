//using Ethiopia.API.Filters;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.OpenApi;

//namespace Ethiopia.API.Extensions;

//public static class ServiceExtensions
//{
//    // ========== Swagger/OpenAPI ==========
//    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
//    {

//        services.AddSwaggerGen(options =>
//        {
//            options.SwaggerDoc("v1", new OpenApiInfo
//            {
//                Title = "Ethiopian.ECommerse",
//                Version = "v1",
//                Description = "eCommerce Platform API for Ethiopian Market",
//                Contact = new OpenApiContact
//                {
//                    Name = "Support Team",
//                    Email = "support@ethiopian-ecommerce.com"
//                },
//                License = new OpenApiLicense
//                {
//                    Name = "Commercial License"
//                }
//            });

//            // ========== XML comments  ==========
//            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
//            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//            if (File.Exists(xmlPath))
//            {
//                options.IncludeXmlComments(xmlPath);
//            }

//            // Add filters
//            options.OperationFilter<AddResponseHeadersFilter>();
//            options.SchemaFilter<MoneySchemaFilter>();


//            // ========== JWT Bearer Authentication ==========
//            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
//            {
//                Type = SecuritySchemeType.Http,
//                Scheme = "bearer",
//                BearerFormat = "JWT",
//                Description = "JWT Authorization header using the Bearer scheme. Example: \'Authorization: Bearer {token}\'",
//                Name = "Authorization",
//                In = ParameterLocation.Header,


//            });

//            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement

//            {

//                [new OpenApiSecuritySchemeReference("bearer", document)] = []
//            });
//        });
//        return services;
//    }

//    public static IServiceCollection AddApiServices(this IServiceCollection services)
//    {
//        // Add API versioning (using Asp.Versioning.Mvc package)
//        services.AddApiVersioning(options =>
//        {
//            options.DefaultApiVersion = new ApiVersion(1, 0);
//            options.AssumeDefaultVersionWhenUnspecified = true;
//            options.ReportApiVersions = true;
//        });

//        // Add versioned API explorer
//        services.AddVersionedApiExplorer(options =>
//        {
//            options.GroupNameFormat = "'v'VVV";
//            options.SubstituteApiVersionInUrl = true;
//        });

//        return services;
//    }

//}