using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ErpPortal.Application.Services;
using ErpPortal.Domain.Entities;
using ErpPortal.Infrastructure.Data;
using ErpPortal.Application.Interfaces.Services;

namespace ErpPortal.Infrastructure.Services
{
    public class WorkCenterService : IWorkCenterService
    {
        private readonly ErpPortalDbContext _context;

        public WorkCenterService(ErpPortalDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkCenter>> GetAllWorkCentersAsync()
        {
            return await _context.WorkCenters
                .ToListAsync();
        }

        public async Task<WorkCenter?> GetWorkCenterByIdAsync(int id)
        {
            return await _context.WorkCenters
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<WorkCenter?> GetWorkCenterByCodeAsync(string code)
        {
            return await _context.WorkCenters
                .FirstOrDefaultAsync(w => w.Code == code);
        }

        public async Task<WorkCenter> CreateWorkCenterAsync(WorkCenter workCenter)
        {
            _context.WorkCenters.Add(workCenter);
            await _context.SaveChangesAsync();
            return workCenter;
        }

        public async Task<WorkCenter> UpdateWorkCenterAsync(WorkCenter workCenter)
        {
            _context.Entry(workCenter).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return workCenter;
        }

        public async Task DeleteWorkCenterAsync(int id)
        {
            var workCenter = await _context.WorkCenters.FindAsync(id);
            if (workCenter != null)
            {
                _context.WorkCenters.Remove(workCenter);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetWorkCenterUsersAsync(int workCenterId)
        {
            var workCenter = await _context.WorkCenters
                .Include(w => w.UserWorkCenters)
                .ThenInclude(uwc => uwc.User)
                .FirstOrDefaultAsync(w => w.Id == workCenterId);

            return workCenter?.UserWorkCenters.Select(uwc => uwc.User).ToList() ?? new List<User>();
        }
    }
} 