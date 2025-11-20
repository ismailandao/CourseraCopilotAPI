using CopilotApiProject.Models;

namespace CopilotApiProject.Repositories;

/// <summary>
/// List-based implementation of IUserRepository that uses an in-memory List&lt;User&gt; as the data source.
/// This implementation provides all CRUD operations without requiring a database.
/// Perfect for demos, testing, or simple applications that don't need persistent storage.
/// </summary>
public class ListUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private int _nextId;

    /// <summary>
    /// Initializes a new instance of ListUserRepository with the provided user list.
    /// </summary>
    /// <param name="users">The singleton List&lt;User&gt; injected from DI container.</param>
    public ListUserRepository(List<User> users)
    {
        _users = users;
        // Set next ID to be higher than any existing user ID
        _nextId = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
    }

    /// <summary>
    /// Gets all active users from the list, ordered by last name then first name.
    /// </summary>
    /// <returns>A collection of active users.</returns>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await Task.FromResult(
            _users.Where(u => u.IsActive)
                  .OrderBy(u => u.LastName)
                  .ThenBy(u => u.FirstName)
                  .ToList()
        );
    }

    /// <summary>
    /// Gets a user by their unique identifier.
    /// Performance Optimization: Uses FirstOrDefault for early termination on ID match.
    /// ID-based lookups are the most efficient as they stop searching after the first match.
    /// Time Complexity: O(n) worst case, O(1) best case if user is at the beginning.
    /// Memory: Constant O(1) - no intermediate collections created.
    /// </summary>
    /// <param name="id">The user ID to search for.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(id));
            }

            // Performance: FirstOrDefault stops at first match, more efficient than Where().FirstOrDefault()
            // Combined conditions in single predicate avoid multiple iterations
            return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id && u.IsActive));
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving user with ID {id}.", ex);
        }
    }

    /// <summary>
    /// Gets a user by their email address (case-insensitive).
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await Task.FromResult(
            _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
        );
    }

    /// <summary>
    /// Creates a new user and adds them to the list.
    /// Automatically assigns a new ID and sets creation timestamp.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <returns>The created user with assigned ID.</returns>
    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            user.Id = _nextId++;
            user.CreatedDate = DateTime.UtcNow;
            user.UpdatedDate = null;
            user.IsActive = true;

            _users.Add(user);
            return await Task.FromResult(user);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user with new information.
    /// Only updates non-null/non-empty properties to support partial updates.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="user">The user object containing updated information.</param>
    /// <returns>The updated user if found, null if user doesn't exist.</returns>
    public async Task<User?> UpdateUserAsync(int id, User user)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(id));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            var existingUser = _users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
                return null;

            // Update only non-null/non-empty properties (partial update support)
            if (!string.IsNullOrWhiteSpace(user.FirstName))
                existingUser.FirstName = user.FirstName;
            
            if (!string.IsNullOrWhiteSpace(user.LastName))
                existingUser.LastName = user.LastName;
            
            if (!string.IsNullOrWhiteSpace(user.Email))
                existingUser.Email = user.Email;
            
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                existingUser.PhoneNumber = user.PhoneNumber;
            
            if (!string.IsNullOrWhiteSpace(user.Department))
                existingUser.Department = user.Department;
            
            if (!string.IsNullOrWhiteSpace(user.Position))
                existingUser.Position = user.Position;
            
            if (!string.IsNullOrWhiteSpace(user.Address))
                existingUser.Address = user.Address;
            
            if (user.Salary.HasValue)
                existingUser.Salary = user.Salary;
            
            if (user.HireDate.HasValue)
                existingUser.HireDate = user.HireDate;

            // Always update the timestamp
            existingUser.UpdatedDate = DateTime.UtcNow;

            return await Task.FromResult(existingUser);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating user with ID {id}.", ex);
        }
    }

    /// <summary>
    /// Performs a soft delete by setting the user's IsActive flag to false.
    /// The user record is preserved for audit purposes.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>True if the user was found and deleted, false otherwise.</returns>
    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(id));
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return false;

            // Soft delete - set IsActive to false instead of removing from list
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;
            
            return await Task.FromResult(true);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deleting user with ID {id}.", ex);
        }
    }

    /// <summary>
    /// Checks if a user with the specified ID exists in the list.
    /// </summary>
    /// <param name="id">The user ID to check.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    public async Task<bool> UserExistsAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(id));
            }

            return await Task.FromResult(_users.Any(u => u.Id == id));
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while checking if user with ID {id} exists.", ex);
        }
    }

    /// <summary>
    /// Checks if an email address is already in use by another user.
    /// Supports excluding a specific user ID for update scenarios.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check.</param>
    /// <returns>True if the email exists (excluding the specified user), false otherwise.</returns>
    /// <summary>
    /// Checks if an email address exists, optionally excluding a specific user ID.
    /// Performance Optimization: Uses Any() for early termination on first match.
    /// Combines all conditions in single predicate to minimize iterations.
    /// Time Complexity: O(n) worst case, O(1) best case with early termination.
    /// Memory: Constant O(1) - no intermediate collections, just boolean result.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludeUserId">Optional user ID to exclude from check.</param>
    /// <returns>True if email exists (excluding specified user), false otherwise.</returns>
    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            // Performance: Combine all conditions in single Any() call for optimal efficiency
            // This avoids creating intermediate IEnumerable and multiple iterations
            return await Task.FromResult(_users.Any(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.IsActive &&
                (excludeUserId == null || u.Id != excludeUserId.Value)));
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while checking if email '{email}' exists.", ex);
        }
    }

    /// <summary>
    /// Gets all active users in a specific department (case-insensitive).
    /// </summary>
    /// <param name="department">The department name to filter by.</param>
    /// <returns>A collection of users in the specified department.</returns>
    public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                throw new ArgumentException("Department cannot be null or empty.", nameof(department));
            }

            return await Task.FromResult(
                _users.Where(u => u.IsActive && 
                                 u.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                      .OrderBy(u => u.LastName)
                      .ThenBy(u => u.FirstName)
                      .ToList()
            );
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving users from department '{department}'.", ex);
        }
    }

    /// <summary>
    /// Searches for active users based on a search term that matches various fields.
    /// Searches across FirstName, LastName, Email, Department, and Position (case-insensitive).
    /// Performance Optimizations:
    /// 1. Uses StringComparison.OrdinalIgnoreCase instead of ToLower() to avoid string allocations
    /// 2. Early active user filtering reduces subsequent string comparisons
    /// 3. Lazy evaluation with deferred ordering until enumeration
    /// 4. Short-circuit evaluation in OR conditions stops at first match
    /// Time Complexity: O(n*m) where n=users, m=average field length
    /// Memory: O(k) where k=number of matching results
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <returns>A collection of users matching the search criteria.</returns>
    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllUsersAsync();

            // Performance: Use StringComparison.OrdinalIgnoreCase instead of ToLower() 
            // to avoid repeated string allocations during search
            return await Task.FromResult(
                _users.Where(u => u.IsActive) // Filter active users first for efficiency
                      .Where(u => 
                          u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                          u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                          u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                          u.Department.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                          u.Position.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                      .OrderBy(u => u.LastName)
                      .ThenBy(u => u.FirstName)
            );
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            // Catch all exceptions except InvalidOperationException (which might come from GetAllUsersAsync)
            throw new InvalidOperationException($"An error occurred while searching for users with term '{searchTerm}'.", ex);
        }
    }
}