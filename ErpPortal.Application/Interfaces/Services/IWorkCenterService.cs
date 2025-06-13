using System.Collections.Generic;
using System.Threading.Tasks;
using ErpPortal.Domain.Entities;

namespace ErpPortal.Application.Interfaces.Services
{
    public interface IWorkCenterService
    {
        Task<List<WorkCenter>> GetAllWorkCentersAsync();
        Task<WorkCenter?> GetWorkCenterByIdAsync(int id);
        Task<WorkCenter?> GetWorkCenterByCodeAsync(string code);
        Task<WorkCenter> CreateWorkCenterAsync(WorkCenter workCenter);
        Task<WorkCenter> UpdateWorkCenterAsync(WorkCenter workCenter);
        Task DeleteWorkCenterAsync(int id);
        Task<List<User>> GetWorkCenterUsersAsync(int workCenterId);
    }
} 