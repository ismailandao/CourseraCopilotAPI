using CopilotApiProject.Models;

namespace CopilotApiProject.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<User?> UpdateUserAsync(int id, User user);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> UserExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
}