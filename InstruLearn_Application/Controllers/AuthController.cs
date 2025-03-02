using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(IAuthService authService, ApplicationDbContext dbContext)
        {
            _authService = authService;
            _dbContext = dbContext;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO userRegisterDTO)
        {
            var result = await _authService.RegisterAsync(userRegisterDTO);
            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO userLoginDTO)
        {
            var result = await _authService.LoginAsync(userLoginDTO);
            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var accountId = User.FindFirst(JwtRegisteredClaimNames.NameId)?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(role))
            {
                return Unauthorized(new ResponseDTO { IsSucceed = false, Message = "Unauthorized access" });
            }

            object userProfile = role switch
            {
                "Admin" => await _dbContext.Admins
                    .Where(a => a.AccountId == accountId)
                    .Select(a => new { a.AdminId, a.Fullname, a.Account.Email, a.Account.Username, Role = role })
                    .FirstOrDefaultAsync(),

                "Staff" => await _dbContext.Staffs
                    .Where(s => s.AccountId == accountId)
                    .Select(s => new { s.StaffId, s.Fullname, s.Account.Email, s.Account.Username, Role = role })
                    .FirstOrDefaultAsync(),

                "Teacher" => await _dbContext.Teachers
                    .Where(t => t.AccountId == accountId)
                    .Select(t => new { t.TeacherId, t.Fullname, t.Heading, t.Details, t.Links, t.Account.Email, t.Account.Username, Role = role })
                    .FirstOrDefaultAsync(),

                "Manager" => await _dbContext.Managers
                    .Where(m => m.AccountId == accountId)
                    .Select(m => new { m.ManagerId, m.Fullname, m.Account.Email, m.Account.Username, Role = role })
                    .FirstOrDefaultAsync(),

                "Learner" => await _dbContext.Learners
                    .Where(l => l.AccountId == accountId)
                    .Select(l => new { l.LearnerId, l.FullName, l.PhoneNumber, l.Account.Email, l.Account.Username, Role = role })
                    .FirstOrDefaultAsync(),

                _ => null
            };

            if (userProfile == null)
            {
                return NotFound(new ResponseDTO { IsSucceed = false, Message = $"{role} profile not found" });
            }

            return Ok(new ResponseDTO
            {
                IsSucceed = true,
                Message = $"{role} profile retrieved successfully",
                Data = userProfile
            });
        } 

    }
}
