using Microsoft.EntityFrameworkCore;
using CopilotApiProject.Models;

namespace CopilotApiProject.Data;

/// <summary>
/// Entity Framework DbContext for the TechHive User Management System.
/// This class handles database operations, entity configurations, and seed data
/// for the User Management API.
/// </summary>
public class UserManagementContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the UserManagementContext class.
    /// </summary>
    /// <param name="options">The options to configure the database context.</param>
    public UserManagementContext(DbContextOptions<UserManagementContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Users DbSet for managing User entities.
    /// This provides access to all user records in the database with full CRUD operations.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Configures the entity models and their relationships when the context is being created.
    /// This method sets up database constraints, indexes, default values, and seed data
    /// to ensure proper database structure and initial test data.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity properties and constraints
        modelBuilder.Entity<User>(entity =>
        {
            // Set primary key
            entity.HasKey(e => e.Id);
            
            // Create unique index on Email to prevent duplicate email addresses
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Configure required string properties with maximum lengths
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Position).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(200);
            
            // Configure decimal property with precision and scale for monetary values
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            
            // Configure datetime properties with datetime2 SQL type for better precision
            entity.Property(e => e.CreatedDate).HasColumnType("datetime2");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime2");
            entity.Property(e => e.HireDate).HasColumnType("datetime2");

            // Set default values for audit and status fields
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Seed initial test data for development and demonstration purposes
        // These users represent typical employees from different departments
        modelBuilder.Entity<User>().HasData(
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
            }
        );
    }
}