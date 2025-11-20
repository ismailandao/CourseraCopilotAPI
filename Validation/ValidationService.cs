using CopilotApiProject.DTOs;
using System.ComponentModel.DataAnnotations;

namespace CopilotApiProject.Validation;

/// <summary>
/// Service for performing comprehensive validation on user data beyond basic attribute validation.
/// Provides business logic validation, cross-field validation, and data integrity checks.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a create user request for business logic rules
    /// </summary>
    /// <param name="createUserDto">User creation data to validate</param>
    /// <returns>Validation result with any errors found</returns>
    ValidationResult ValidateCreateUser(CreateUserDto createUserDto);

    /// <summary>
    /// Validates an update user request for business logic rules
    /// </summary>
    /// <param name="updateUserDto">User update data to validate</param>
    /// <param name="existingUserId">ID of the existing user being updated</param>
    /// <returns>Validation result with any errors found</returns>
    ValidationResult ValidateUpdateUser(UpdateUserDto updateUserDto, int existingUserId);

    /// <summary>
    /// Validates that salary is appropriate for the given position and department
    /// </summary>
    /// <param name="salary">Salary to validate</param>
    /// <param name="position">Job position</param>
    /// <param name="department">Department</param>
    /// <returns>True if salary is reasonable for the role, false otherwise</returns>
    bool ValidateSalaryForPosition(decimal? salary, string position, string department);

    /// <summary>
    /// Validates that the hire date makes sense in relation to other dates
    /// </summary>
    /// <param name="hireDate">Hire date to validate</param>
    /// <param name="existingCreatedDate">Existing user creation date (for updates)</param>
    /// <returns>True if hire date is logical, false otherwise</returns>
    bool ValidateHireDateLogic(DateTime? hireDate, DateTime? existingCreatedDate = null);
}

/// <summary>
/// Implementation of comprehensive validation service
/// </summary>
public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;

    // Salary ranges by position type (simplified mapping)
    private static readonly Dictionary<string, (decimal Min, decimal Max)> PositionSalaryRanges = new()
    {
        { "analyst", (45000, 90000) },
        { "developer", (60000, 120000) },
        { "manager", (70000, 150000) },
        { "director", (100000, 200000) },
        { "administrator", (50000, 100000) },
        { "specialist", (55000, 95000) },
        { "coordinator", (40000, 75000) },
        { "assistant", (30000, 60000) }
    };

    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger;
    }

    public ValidationResult ValidateCreateUser(CreateUserDto createUserDto)
    {
        var errors = new List<string>();

        try
        {
            // Cross-field validation
            if (!ValidateNameConsistency(createUserDto.FirstName, createUserDto.LastName))
            {
                errors.Add("First name and last name appear to be inconsistent or potentially fake.");
            }

            // Email domain validation for business context
            if (!ValidateEmailDomainForDepartment(createUserDto.Email, createUserDto.Department))
            {
                errors.Add("Email domain may not be appropriate for the specified department.");
            }

            // Salary validation
            if (!ValidateSalaryForPosition(createUserDto.Salary, createUserDto.Position, createUserDto.Department))
            {
                errors.Add("Salary appears to be outside the typical range for this position and department.");
            }

            // Hire date validation
            if (!ValidateHireDateLogic(createUserDto.HireDate))
            {
                errors.Add("Hire date is invalid or inconsistent.");
            }

            // Phone number uniqueness (simplified check)
            if (!string.IsNullOrEmpty(createUserDto.PhoneNumber) && IsCommonTestPhoneNumber(createUserDto.PhoneNumber))
            {
                errors.Add("Please provide a real phone number, not a test number.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during user validation");
            errors.Add("Validation error occurred. Please review your input.");
        }

        return new ValidationResult(errors.Count == 0, errors);
    }

    public ValidationResult ValidateUpdateUser(UpdateUserDto updateUserDto, int existingUserId)
    {
        var errors = new List<string>();

        try
        {
            // Name consistency check (if both names are being updated)
            if (!string.IsNullOrEmpty(updateUserDto.FirstName) && !string.IsNullOrEmpty(updateUserDto.LastName))
            {
                if (!ValidateNameConsistency(updateUserDto.FirstName, updateUserDto.LastName))
                {
                    errors.Add("Updated names appear to be inconsistent or potentially fake.");
                }
            }

            // Email validation (if being updated)
            if (!string.IsNullOrEmpty(updateUserDto.Email) && !string.IsNullOrEmpty(updateUserDto.Department))
            {
                if (!ValidateEmailDomainForDepartment(updateUserDto.Email, updateUserDto.Department))
                {
                    errors.Add("Updated email domain may not be appropriate for the specified department.");
                }
            }

            // Salary validation (if being updated)
            if (updateUserDto.Salary.HasValue)
            {
                var position = updateUserDto.Position ?? "Unknown";
                var department = updateUserDto.Department ?? "Unknown";
                
                if (!ValidateSalaryForPosition(updateUserDto.Salary, position, department))
                {
                    errors.Add("Updated salary appears to be outside the typical range for this position.");
                }
            }

            // Active status validation
            if (updateUserDto.IsActive.HasValue && !updateUserDto.IsActive.Value)
            {
                // Additional checks could be added here for deactivating users
                // e.g., checking for dependent records, outstanding tasks, etc.
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during user update validation");
            errors.Add("Validation error occurred. Please review your input.");
        }

        return new ValidationResult(errors.Count == 0, errors);
    }

    public bool ValidateSalaryForPosition(decimal? salary, string position, string department)
    {
        if (!salary.HasValue || salary.Value <= 0)
            return true; // No salary specified is acceptable

        // Normalize position for lookup
        var normalizedPosition = position.ToLower();
        
        // Find matching salary range
        var matchingRange = PositionSalaryRanges
            .FirstOrDefault(kvp => normalizedPosition.Contains(kvp.Key));

        if (matchingRange.Key != null)
        {
            return salary.Value >= matchingRange.Value.Min && salary.Value <= matchingRange.Value.Max;
        }

        // If no specific range found, use general bounds
        return salary.Value >= 25000 && salary.Value <= 300000;
    }

    public bool ValidateHireDateLogic(DateTime? hireDate, DateTime? existingCreatedDate = null)
    {
        if (!hireDate.HasValue)
            return true; // No hire date is acceptable

        var hire = hireDate.Value;
        var today = DateTime.Today;
        var minimumDate = new DateTime(1950, 1, 1);

        // Basic date range validation
        if (hire < minimumDate || hire > today)
            return false;

        // If updating, hire date should be before or equal to creation date
        if (existingCreatedDate.HasValue && hire > existingCreatedDate.Value.Date)
            return false;

        return true;
    }

    #region Private Helper Methods

    private static bool ValidateNameConsistency(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return false;

        // Check for obvious test names
        var fullName = $"{firstName} {lastName}".ToLower();
        var testNames = new[] 
        { 
            "test user", "john doe", "jane doe", "first last", 
            "user name", "admin user", "sample user" 
        };

        return !testNames.Contains(fullName);
    }

    private static bool ValidateEmailDomainForDepartment(string email, string department)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(department))
            return true; // Skip validation if either is empty

        // This is a simplified check - in reality, you might have more sophisticated rules
        var domain = email.Split('@').LastOrDefault()?.ToLower();
        
        // Check for obvious test domains
        var testDomains = new[] { "test.com", "example.com", "fake.com", "dummy.com" };
        
        return !testDomains.Contains(domain);
    }

    private static bool IsCommonTestPhoneNumber(string phoneNumber)
    {
        var cleanNumber = phoneNumber.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");
        
        var testNumbers = new[]
        {
            "1234567890", "0123456789", "5555555555", 
            "1111111111", "0000000000", "9999999999"
        };

        return testNumbers.Contains(cleanNumber);
    }

    #endregion
}

/// <summary>
/// Result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    public ValidationResult(bool isValid, List<string> errors)
    {
        IsValid = isValid;
        Errors = errors?.AsReadOnly() ?? new List<string>().AsReadOnly();
    }
}