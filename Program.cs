using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CopilotApiProject.Data;
using CopilotApiProject.Repositories;
using CopilotApiProject.Services;
using CopilotApiProject.Models;
using CopilotApiProject.Validation;
using CopilotApiProject.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// ===== PERFORMANCE OPTIMIZATION - MEMORY CACHING =====
// Configure in-memory caching for frequently accessed data
// This reduces computation overhead for repeated queries like:
// - User lookups by ID/email (cached for 5 minutes)
// - Department-based searches (cached for 10 minutes)
// - Search results (cached for 2 minutes due to volatility)
// Memory impact: ~1-5MB for typical datasets, significant performance gain
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000; // Limit cache to 1000 entries
    options.CompactionPercentage = 0.25; // Remove 25% when limit reached
});

// ===== DATABASE CONFIGURATION =====
// Configure data source for User Management System
// Current setup uses List-based data source for simplicity and demo purposes
// This provides fast access and no external dependencies

// OPTION 1: List-based Data Source (Current - Main data source)
// Pros: Simple, fast, no database setup required, great for demos
// Cons: Data lost on restart, not suitable for large datasets
// Register in-memory list as a singleton data source
builder.Services.AddSingleton<List<User>>(serviceProvider =>
{
    // Initialize with sample data for TechHive Solutions
    return new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@techhive.com",
            PhoneNumber = "555-0101",
            Department = "IT",
            Position = "Software Developer",
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            Address = "123 Tech Street, Silicon Valley",
            Salary = 75000m,
            HireDate = new DateTime(2023, 1, 15)
        },
        new User
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@techhive.com",
            PhoneNumber = "555-0102",
            Department = "HR",
            Position = "HR Manager",
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            Address = "456 Business Blvd, Corporate City",
            Salary = 85000m,
            HireDate = new DateTime(2022, 8, 20)
        },
        new User
        {
            Id = 3,
            FirstName = "Mike",
            LastName = "Johnson",
            Email = "mike.johnson@techhive.com",
            PhoneNumber = "555-0103",
            Department = "IT",
            Position = "System Administrator",
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            Address = "789 Network Lane, Server Town",
            Salary = 70000m,
            HireDate = new DateTime(2023, 3, 10)
        },
        new User
        {
            Id = 4,
            FirstName = "Donald",
            LastName = "Wilson",
            Email = "donald.wilson@techhive.com",
            PhoneNumber = "555-0104",
            Department = "Finance",
            Position = "Financial Analyst",
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            Address = "321 Finance Plaza, Business District",
            Salary = 72000m,
            HireDate = new DateTime(2020, 8, 13)
        }
    };
});

// OPTION 2: In-Memory Database (Alternative - Development/Testing)
// Uncomment below and comment above for Entity Framework approach
// Pros: EF features, migrations, better for complex queries
// Cons: More overhead, requires EF knowledge
/*
builder.Services.AddDbContext<UserManagementContext>(options =>
    options.UseInMemoryDatabase("UserManagementDB"));
*/

// OPTION 3: SQL Server Database (Production)
// Uncomment below for production use with persistent data
// Replace connection string with your actual SQL Server instance
/*
builder.Services.AddDbContext<UserManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
*/

// ===== PERFORMANCE-OPTIMIZED DEPENDENCY INJECTION =====
// Register repositories and services with optimal lifetime management

// Register List-based repository (works with List<User> data source)
// Performance: Registered as Scoped instead of Singleton for thread safety
// Each request gets its own repository instance while maintaining reasonable memory usage
builder.Services.AddScoped<IUserRepository, ListUserRepository>();

// Register business logic service with intelligent caching decorator
// Performance: Decorator pattern provides caching without changing core business logic
// The CachedUserService wraps UserService to add performance optimizations
builder.Services.AddScoped<UserService>(); // Base implementation
builder.Services.AddScoped<IUserService>(provider =>
{
    var baseService = provider.GetRequiredService<UserService>();
    var cache = provider.GetRequiredService<IMemoryCache>();
    var logger = provider.GetRequiredService<ILogger<CachedUserService>>();
    return new CachedUserService(baseService, cache, logger);
});

// Register validation service for comprehensive data validation
// Performance: Scoped to minimize validation overhead and memory usage
builder.Services.AddScoped<IValidationService, ValidationService>();

// Add JWT token service for authentication
builder.Services.AddScoped<JwtTokenService>();

// Note: When switching to Entity Framework, change to:
// builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "TechHive User Management API", 
        Version = "v1",
        Description = "A comprehensive API for managing user records in TechHive Solutions internal tools"
    });
});

// Add CORS policy for cross-origin requests (if needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== DATA SOURCE INITIALIZATION =====
// Initialize data source (not needed for List-based approach)
// The List<User> singleton is automatically initialized when first accessed
// Data is populated through the singleton registration above

// Note: Uncomment below section when using Entity Framework DbContext
/*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserManagementContext>();
    context.Database.EnsureCreated();
    
    // For production with SQL Server, consider using:
    // context.Database.Migrate(); // Applies pending migrations
    // This is safer than EnsureCreated() for production databases
}
*/

// Configure the HTTP request pipeline

// Add error handling middleware (should be first to catch all exceptions)
app.UseErrorHandling();

// Add token validation middleware for JWT authentication
app.UseTokenValidation();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechHive User Management API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

// Add request/response logging middleware
app.UseRequestResponseLogging();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Service = "TechHive User Management API", 
    Timestamp = DateTime.UtcNow 
});

// Welcome message
app.MapGet("/api", () => new { 
    Message = "Welcome to TechHive User Management API", 
    Documentation = "/swagger",
    Version = "1.0.0",
    Endpoints = new[] {
        "GET /api/users - Get all users",
        "GET /api/users/{id} - Get user by ID", 
        "GET /api/users/email/{email} - Get user by email",
        "GET /api/users/department/{department} - Get users by department",
        "GET /api/users/search?searchTerm={term} - Search users",
        "POST /api/users - Create new user",
        "PUT /api/users/{id} - Update user",
        "DELETE /api/users/{id} - Delete user"
    }
});

app.Run();