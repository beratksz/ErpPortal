using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Domain.Entities;
using ErpPortal.Application.Models;
using Microsoft.Extensions.Logging;
using ErpPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ErpPortal.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly ErpPortalDbContext _context;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger, ErpPortalDbContext context)
        {
            _userRepository = userRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return null;
                }

                // TODO: Implement proper password hashing
                if (user.Password != password)
                {
                    _logger.LogWarning("Invalid password for user: {Username}", username);
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user: {Username}", username);
                throw;
            }
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                return await _userRepository.GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
            }
        }

        public async Task<bool> HasWorkCenterAccessAsync(int userId, string workCenterCode)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return false;
                }

                if (user.IsAdmin)
                {
                    return true;
                }

                return await _userRepository.HasWorkCenterAccessAsync(userId, workCenterCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking work center access for user: {UserId}, work center: {WorkCenterCode}", userId, workCenterCode);
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                return await _userRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by id: {UserId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _userRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<User> AddAsync(User user)
        {
            try
            {
                // TODO: Implement proper password hashing
                return await _userRepository.AddAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user: {Username}", user.Username);
                throw;
            }
        }

        public async Task<User> UpdateAsync(User user)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(user.Id);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException($"User not found with id: {user.Id}");
                }

                // TODO: Implement proper password hashing if password is being updated
                return await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User not found with id: {id}");
                }

                await _userRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                throw;
            }
        }

        // Additional methods required by IUserService
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                return await GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                // Hash password before saving
                return await AddAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", user.Username);
                throw;
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                return await UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                await DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task AssignWorkCenterAsync(int userId, int workCenterId)
        {
            var existingAssignment = await _context.UserWorkCenters
                .AnyAsync(uw => uw.UserId == userId && uw.WorkCenterId == workCenterId);

            if (!existingAssignment)
            {
                var user = await _context.Users.FindAsync(userId);
                var workCenter = await _context.WorkCenters.FindAsync(workCenterId);

                if (user != null && workCenter != null)
                {
                    _context.UserWorkCenters.Add(new UserWorkCenter { UserId = userId, WorkCenterId = workCenterId });
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveWorkCenterAsync(int userId, int workCenterId)
        {
            var assignment = await _context.UserWorkCenters
                .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WorkCenterId == workCenterId);

            if (assignment != null)
            {
                _context.UserWorkCenters.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }
    }
} 