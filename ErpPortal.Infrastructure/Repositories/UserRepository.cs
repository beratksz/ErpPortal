using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Domain.Entities;
using ErpPortal.Infrastructure.Data;

namespace ErpPortal.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ErpPortalDbContext _context;

        public UserRepository(ErpPortalDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserWorkCenters)
                .ThenInclude(uwc => uwc.WorkCenter)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserWorkCenters)
                .ThenInclude(uwc => uwc.WorkCenter)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public Task<bool> HasWorkCenterAccessAsync(int userId, string workCenterCode)
        {
            // Geçici olarak basitleştirildi - hep true döndürür
            return Task.FromResult(true);
        }

        public async Task<IEnumerable<User>> GetByWorkCenterAsync(string workCenterCode)
        {
            // Geçici olarak tüm kullanıcıları döndürür
            return await _context.Users.ToListAsync();
        }
    }
}
