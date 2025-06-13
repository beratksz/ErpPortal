using ErpPortal.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ErpPortal.Application.Interfaces.Repositories
{
    public interface IWorkLogRepository
    {
        Task AddAsync(WorkLog log);
        Task UpdateAsync(WorkLog log);
        Task<WorkLog?> GetLatestLogAsync(string orderNo, int operationNo, int userId);
        Task<IEnumerable<WorkLog>> GetLogsForOperationAsync(string orderNo, int operationNo);
        Task<int> SaveChangesAsync();
    }
} 