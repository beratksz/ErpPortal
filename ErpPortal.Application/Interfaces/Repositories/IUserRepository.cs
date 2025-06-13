using ErpPortal.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpPortal.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<bool> HasWorkCenterAccessAsync(int userId, string workCenterCode);
        Task<IEnumerable<User>> GetByWorkCenterAsync(string workCenterCode);
    }
}
