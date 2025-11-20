using CopilotApiProject.DTOs;
using CopilotApiProject.Models;

namespace CopilotApiProject.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(int id);
    Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(string department);
    Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm);
}