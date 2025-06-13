using ErpPortal.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpPortal.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> ValidateUserAsync(string username, string password);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<bool> HasWorkCenterAccessAsync(int userId, string workCenterCode);
        
        // Additional methods needed by AccountApiController
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task AssignWorkCenterAsync(int userId, int workCenterId);
        Task RemoveWorkCenterAsync(int userId, int workCenterId);
    }
} 