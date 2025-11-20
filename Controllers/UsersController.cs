using Microsoft.AspNetCore.Mvc;
using CopilotApiProject.DTOs;
using CopilotApiProject.Services;
using System.ComponentModel.DataAnnotations;

namespace CopilotApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all active users</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for GetUser with ID {UserId}: {ErrorMessage}", id, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving user with ID {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User details</returns>
    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
    {
        // Enhanced email validation
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email cannot be empty");
        }

        // Basic email format validation
        if (!IsValidEmailFormat(email))
        {
            return BadRequest("Invalid email format");
        }

        // Check for potentially malicious input
        if (email.Length > 100 || ContainsSqlInjectionPatterns(email))
        {
            return BadRequest("Invalid email address");
        }

        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"User with email {email} not found");
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for GetUserByEmail with email {Email}: {ErrorMessage}", email, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving user with email {Email}", email);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Get users by department
    /// </summary>
    /// <param name="department">Department name</param>
    /// <returns>List of users in the specified department</returns>
    [HttpGet("department/{department}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByDepartment(string department)
    {
        // Enhanced department validation
        if (string.IsNullOrWhiteSpace(department))
        {
            return BadRequest("Department cannot be empty");
        }

        // Sanitize and validate department input
        department = department.Trim();
        if (department.Length > 50 || ContainsSqlInjectionPatterns(department))
        {
            return BadRequest("Invalid department name");
        }

        // Check for valid characters in department name
        if (!System.Text.RegularExpressions.Regex.IsMatch(department, @"^[a-zA-Z\s\&]+$"))
        {
            return BadRequest("Department name can only contain letters, spaces, and ampersands");
        }

        try
        {
            var users = await _userService.GetUsersByDepartmentAsync(department);
            return Ok(users);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for GetUsersByDepartment with department {Department}: {ErrorMessage}", department, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving users from department {Department}", department);
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Search users by name, email, department, or position
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching users</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers([FromQuery] string searchTerm)
    {
        // Enhanced search term validation
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty");
        }

        // Sanitize and validate search input
        searchTerm = searchTerm.Trim();
        
        // Prevent excessively long search terms
        if (searchTerm.Length > 100)
        {
            return BadRequest("Search term is too long (maximum 100 characters)");
        }

        // Prevent search terms that are too short
        if (searchTerm.Length < 2)
        {
            return BadRequest("Search term must be at least 2 characters long");
        }

        // Check for SQL injection patterns
        if (ContainsSqlInjectionPatterns(searchTerm))
        {
            return BadRequest("Invalid search term");
        }

        // Allow only safe characters in search terms
        if (!System.Text.RegularExpressions.Regex.IsMatch(searchTerm, @"^[a-zA-Z0-9\s\@\-\.\\_]+$"))
        {
            return BadRequest("Search term contains invalid characters");
        }

        try
        {
            var users = await _userService.SearchUsersAsync(searchTerm);
            return Ok(users);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for SearchUsers with term {SearchTerm}: {ErrorMessage}", searchTerm, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while searching users with term {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching users");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createUserDto">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdUser = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(
                nameof(GetUser),
                new { id = createdUser.Id },
                createdUser);
        }
        catch (ArgumentException ex)
        {
            // This includes ArgumentNullException as it inherits from ArgumentException
            _logger.LogWarning(ex, "Invalid argument provided for CreateUser: {ErrorMessage}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error occurred while creating user with email {Email}: {ErrorMessage}", createUserDto.Email, ex.Message);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating user with email {Email}", createUserDto.Email);
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="updateUserDto">User update data</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedUser = await _userService.UpdateUserAsync(id, updateUserDto);
            if (updatedUser == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            return Ok(updatedUser);
        }
        catch (ArgumentException ex)
        {
            // This includes ArgumentNullException as it inherits from ArgumentException
            _logger.LogWarning(ex, "Invalid argument provided for UpdateUser with ID {UserId}: {ErrorMessage}", id, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error occurred while updating user with ID {UserId}: {ErrorMessage}", id, ex.Message);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating user with ID {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
            {
                return NotFound($"User with ID {id} not found");
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for DeleteUser with ID {UserId}: {ErrorMessage}", id, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting user with ID {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Validates email format using built-in .NET validation
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid, false otherwise</returns>
    private static bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

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

    /// <summary>
    /// Checks for common SQL injection patterns in user input
    /// </summary>
    /// <param name="input">Input string to validate</param>
    /// <returns>True if potentially malicious patterns are found, false otherwise</returns>
    private static bool ContainsSqlInjectionPatterns(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var lowerInput = input.ToLower();
        
        // Common SQL injection patterns
        var sqlPatterns = new[]
        {
            "' or '1'='1", "' or 1=1", "'; drop table", "'; delete from", 
            "'; insert into", "'; update ", "'; exec", "'; execute",
            "union select", "union all select", "<script", "javascript:",
            "onload=", "onerror=", "onclick=", "--", "/*", "*/",
            "char(", "cast(", "convert(", "waitfor delay"
        };

        return sqlPatterns.Any(pattern => lowerInput.Contains(pattern));
    }

    #endregion
}