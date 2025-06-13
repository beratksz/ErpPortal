using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Domain.Entities;
using ErpPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ErpPortal.Infrastructure.Repositories
{
    public class WorkLogRepository : IWorkLogRepository
    {
        private readonly ErpPortalDbContext _context;
        public WorkLogRepository(ErpPortalDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(WorkLog log)
        {
            await _context.WorkLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WorkLog log)
        {
            _context.Entry(log).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<WorkLog?> GetLatestLogAsync(string orderNo, int operationNo, int userId)
        {
            return await _context.WorkLogs
                .Where(w => w.OrderNo == orderNo && w.OperationNo == operationNo && w.UserId == userId)
                .OrderByDescending(w => w.StartTime)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WorkLog>> GetLogsForOperationAsync(string orderNo, int operationNo)
        {
            return await _context.WorkLogs
                .Where(w => w.OrderNo == orderNo && w.OperationNo == operationNo)
                .ToListAsync();
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
} 