using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErpPortal.Domain.Entities;

namespace ErpPortal.Application.Interfaces.Repositories
{
    public interface IWorkCenterRepository
    {
        Task<WorkCenter?> GetByIdAsync(int id);
        Task<WorkCenter?> GetByCodeAsync(string code);
        Task<IEnumerable<WorkCenter>> GetAllAsync();
        Task<WorkCenter> AddAsync(WorkCenter workCenter);
        Task<WorkCenter> UpdateAsync(WorkCenter workCenter);
        Task DeleteAsync(int id);
    }
}
