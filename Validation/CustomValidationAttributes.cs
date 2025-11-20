using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CopilotApiProject.Validation;

/// <summary>
/// Custom validation attribute to validate phone number formats.
/// Accepts various common phone number formats including international formats.
/// </summary>
public class PhoneNumberValidationAttribute : ValidationAttribute
{
    private static readonly Regex PhoneRegex = new Regex(
        @"^[\+]?[1-9]?[\d\s\-\(\)]{7,15}$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override bool IsValid(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return true; // Allow null/empty for optional fields
        }

        var phoneNumber = value.ToString()!.Trim();
        
        // Remove common formatting characters for validation
        var cleanedNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        
        return PhoneRegex.IsMatch(phoneNumber) && cleanedNumber.Length >= 7 && cleanedNumber.Length <= 15;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a valid phone number format (7-15 digits, may include spaces, dashes, or parentheses).";
    }
}

/// <summary>
/// Custom validation attribute to ensure hire dates are not in the future
/// and not unreasonably far in the past.
/// </summary>
public class HireDateValidationAttribute : ValidationAttribute
{
    private static readonly DateTime MinimumHireDate = new DateTime(1950, 1, 1);

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true; // Allow null for optional fields
        }

        if (value is DateTime hireDate)
        {
            var today = DateTime.Today;
            return hireDate >= MinimumHireDate && hireDate <= today;
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be between {MinimumHireDate:yyyy-MM-dd} and today's date.";
    }
}

/// <summary>
/// Custom validation attribute for department names.
/// Ensures departments are from a predefined list of valid departments.
/// </summary>
public class DepartmentValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<string> ValidDepartments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "IT", "Information Technology",
        "HR", "Human Resources", 
        "Finance", "Accounting",
        "Marketing", "Sales",
        "Operations", "Engineering",
        "Legal", "Administration",
        "Customer Service", "Support",
        "Research", "Development", "R&D",
        "Quality Assurance", "QA",
        "Security", "Management"
    };

    public override bool IsValid(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return false; // Department is required
        }

        var department = value.ToString()!.Trim();
        return ValidDepartments.Contains(department);
    }

    public override string FormatErrorMessage(string name)
    {
        var validDepts = string.Join(", ", ValidDepartments.OrderBy(d => d));
        return $"{name} must be one of the following: {validDepts}.";
    }
}

/// <summary>
/// Custom validation attribute for salary ranges.
/// Ensures salary is within reasonable bounds for the organization.
/// </summary>
public class SalaryRangeValidationAttribute : ValidationAttribute
{
    private const decimal MinSalary = 20000m;  // Minimum wage considerations
    private const decimal MaxSalary = 1000000m; // Executive salary cap

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true; // Allow null for optional salary field
        }

        if (value is decimal salary)
        {
            return salary >= MinSalary && salary <= MaxSalary;
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be between {MinSalary:C0} and {MaxSalary:C0}.";
    }
}

/// <summary>
/// Custom validation attribute to prevent common invalid names.
/// Blocks obvious fake names, numbers, and inappropriate content.
/// </summary>
public class NameValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<string> InvalidNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "test", "user", "admin", "null", "undefined", "name", "firstname", "lastname",
        "john doe", "jane doe", "test user", "dummy", "sample", "example"
    };

    private static readonly Regex NamePattern = new Regex(
        @"^[a-zA-Z\s\-\.\']+$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override bool IsValid(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return false; // Names are required
        }

        var name = value.ToString()!.Trim();

        // Check for invalid patterns
        if (InvalidNames.Contains(name) || 
            !NamePattern.IsMatch(name) ||
            name.Any(char.IsDigit) ||
            name.Length < 2)
        {
            return false;
        }

        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must contain only letters, spaces, hyphens, periods, and apostrophes. Test names and numbers are not allowed.";
    }
}

/// <summary>
/// Custom validation attribute for email domains.
/// Ensures email addresses use approved corporate or common domains.
/// </summary>
public class BusinessEmailValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<string> ValidDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "techhive.com", "gmail.com", "outlook.com", "hotmail.com", 
        "yahoo.com", "company.com", "corporate.com"
    };

    private static readonly HashSet<string> BlockedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "tempmail.com", "10minutemail.com", "guerrillamail.com", 
        "mailinator.com", "throwaway.email"
    };

    public override bool IsValid(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return false; // Email is required
        }

        var email = value.ToString()!.Trim().ToLower();

        if (!IsValidEmailFormat(email))
        {
            return false;
        }

        var domain = email.Split('@').LastOrDefault();
        if (string.IsNullOrEmpty(domain))
        {
            return false;
        }

        // Block temporary email services
        if (BlockedDomains.Contains(domain))
        {
            return false;
        }

        // For strict corporate policy, uncomment this line:
        // return ValidDomains.Contains(domain);

        // For now, allow any domain except blocked ones
        return true;
    }

    private static bool IsValidEmailFormat(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a valid business email address. Temporary email services are not allowed.";
    }
}