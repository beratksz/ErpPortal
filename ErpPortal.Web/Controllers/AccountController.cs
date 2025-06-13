using ErpPortal.Domain.Entities; // User entity için
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ErpPortal.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.Security.Claims;
using ErpPortal.Web.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ErpPortal.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AccountController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.ValidateUserAsync(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre." });
            }

            // Set session for MVC controllers
            HttpContext.Session.SetString("UserName", user.Username);
            var defaultWc = user.UserWorkCenters.FirstOrDefault()?.WorkCenter?.Code;
            if (!string.IsNullOrEmpty(defaultWc))
                HttpContext.Session.SetString("WorkCenterCode", defaultWc);

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new 
                { 
                    user.Username, 
                    user.FullName, 
                    user.IsAdmin,
                    WorkCenters = user.UserWorkCenters.Select(uwc => new { uwc.WorkCenter.Id, uwc.WorkCenter.Code, uwc.WorkCenter.Name }).ToList()
                }
            });
        }

        [HttpGet("users/me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                IsAdmin = user.IsAdmin,
                WorkCenters = user.UserWorkCenters.Select(uwc => new WorkCenterViewModel { Id = uwc.WorkCenter.Id, Code = uwc.WorkCenter.Code, Name = uwc.WorkCenter.Name }).ToList()
            });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                IsAdmin = u.IsAdmin
            }));
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserViewModel>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                IsAdmin = user.IsAdmin,
                WorkCenters = user.UserWorkCenters.Select(uwc => new WorkCenterViewModel { Id = uwc.WorkCenter.Id, Code = uwc.WorkCenter.Code, Name = uwc.WorkCenter.Name }).ToList()
            });
        }

        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserViewModel>> CreateUser([FromBody] CreateUserRequest request)
        {
            var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = $"'{request.Username}' kullanıcı adı zaten kullanımda." });
            }

            var user = new User
            {
                Username = request.Username,
                FullName = request.FullName,
                IsAdmin = request.IsAdmin,
                Password = request.Password // Password should be hashed by the service
            };

            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, new UserViewModel
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                FullName = createdUser.FullName,
                IsAdmin = createdUser.IsAdmin
            });
        }

        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserViewModel>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = request.FullName;
            user.IsAdmin = request.IsAdmin;
            if (!string.IsNullOrEmpty(request.Password))
            {
                // Logic to update password would go here
            }

            var updatedUser = await _userService.UpdateUserAsync(user);
            return Ok(new UserViewModel
            {
                Id = updatedUser.Id,
                Username = updatedUser.Username,
                FullName = updatedUser.FullName,
                IsAdmin = updatedUser.IsAdmin
            });
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userService.DeleteUserAsync(user.Id);
            return NoContent();
        }

        [HttpPost("users/{userId}/workcenters/{workCenterId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignWorkCenter(int userId, int workCenterId)
        {
            await _userService.AssignWorkCenterAsync(userId, workCenterId);
            return NoContent();
        }

        [HttpDelete("users/{userId}/workcenters/{workCenterId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveWorkCenter(int userId, int workCenterId)
        {
            await _userService.RemoveWorkCenterAsync(userId, workCenterId);
            return NoContent();
        }
    }
}
