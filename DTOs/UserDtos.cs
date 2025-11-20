using System.ComponentModel.DataAnnotations;
using CopilotApiProject.Validation;

namespace CopilotApiProject.DTOs;

public class CreateUserDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
    [NameValidation(ErrorMessage = "First name contains invalid characters or format.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
    [NameValidation(ErrorMessage = "Last name contains invalid characters or format.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters.")]
    [BusinessEmailValidation(ErrorMessage = "Please use a valid business email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
    [PhoneNumberValidation(ErrorMessage = "Please provide a valid phone number format.")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(50, ErrorMessage = "Department name cannot exceed 50 characters.")]
    [DepartmentValidation(ErrorMessage = "Please select a valid department.")]
    [Display(Name = "Department")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position/Job title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Position must be between 3 and 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-\.]+$", ErrorMessage = "Position can only contain letters, spaces, hyphens, and periods.")]
    [Display(Name = "Position/Job Title")]
    public string Position { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number.")]
    [SalaryRangeValidation(ErrorMessage = "Salary must be within acceptable company range.")]
    [Display(Name = "Annual Salary")]
    public decimal? Salary { get; set; }

    [HireDateValidation(ErrorMessage = "Hire date must be a valid date not in the future.")]
    [DataType(DataType.Date, ErrorMessage = "Please provide a valid date.")]
    [Display(Name = "Hire Date")]
    public DateTime? HireDate { get; set; }
}

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
    [NameValidation(ErrorMessage = "First name contains invalid characters or format.")]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
    [NameValidation(ErrorMessage = "Last name contains invalid characters or format.")]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters.")]
    [BusinessEmailValidation(ErrorMessage = "Please use a valid business email address.")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
    [PhoneNumberValidation(ErrorMessage = "Please provide a valid phone number format.")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [StringLength(50, ErrorMessage = "Department name cannot exceed 50 characters.")]
    [DepartmentValidation(ErrorMessage = "Please select a valid department.")]
    [Display(Name = "Department")]
    public string? Department { get; set; }

    [StringLength(100, MinimumLength = 3, ErrorMessage = "Position must be between 3 and 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-\.]+$", ErrorMessage = "Position can only contain letters, spaces, hyphens, and periods.")]
    [Display(Name = "Position/Job Title")]
    public string? Position { get; set; }

    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number.")]
    [SalaryRangeValidation(ErrorMessage = "Salary must be within acceptable company range.")]
    [Display(Name = "Annual Salary")]
    public decimal? Salary { get; set; }

    [HireDateValidation(ErrorMessage = "Hire date must be a valid date not in the future.")]
    [DataType(DataType.Date, ErrorMessage = "Please provide a valid date.")]
    [Display(Name = "Hire Date")]
    public DateTime? HireDate { get; set; }

    [Display(Name = "Active Status")]
    public bool? IsActive { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Address { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
}