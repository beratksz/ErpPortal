using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Domain.Entities;
using ErpPortal.Web.Models;

namespace ErpPortal.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            var users = await _userService.GetAllAsync();
            return users.ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            var createdUser = await _userService.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            try
            {
                await _userService.UpdateAsync(user);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{userId}/workcenters/access/{workCenterCode}")]
        public async Task<ActionResult<bool>> CheckWorkCenterAccess(int userId, string workCenterCode)
        {
            var hasAccess = await _userService.HasWorkCenterAccessAsync(userId, workCenterCode);
            return hasAccess;
        }
    }
} 