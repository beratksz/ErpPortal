using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Domain.Entities;
using ErpPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ErpPortal.Infrastructure.Repositories
{
    public class WorkCenterRepository : IWorkCenterRepository
    {
        private readonly ErpPortalDbContext _context;

        public WorkCenterRepository(ErpPortalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WorkCenter>> GetAllAsync()
        {
            return await _context.WorkCenters
                .Include(w => w.Operations)
                .ToListAsync();
        }

        public async Task<WorkCenter?> GetByIdAsync(int id)
        {
            return await _context.WorkCenters
                .Include(w => w.Operations)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<WorkCenter?> GetByCodeAsync(string code)
        {
            return await _context.WorkCenters
                .Include(w => w.Operations)
                .FirstOrDefaultAsync(w => w.Code == code);
        }

        public async Task<IEnumerable<WorkCenter>> GetByUserAsync(int userId)
        {
            return await _context.WorkCenters
                .Where(w => w.UserWorkCenters.Any(uw => uw.UserId == userId))
                .ToListAsync();
        }

        public async Task<WorkCenter> AddAsync(WorkCenter workCenter)
        {
            _context.WorkCenters.Add(workCenter);
            await _context.SaveChangesAsync();
            return workCenter;
        }

        public async Task<WorkCenter> UpdateAsync(WorkCenter workCenter)
        {
            _context.Entry(workCenter).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return workCenter;
        }

        public async Task DeleteAsync(int id)
        {
            var workCenter = await _context.WorkCenters.FindAsync(id);
            if (workCenter != null)
            {
                _context.WorkCenters.Remove(workCenter);
                await _context.SaveChangesAsync();
            }
        }
    }
}
