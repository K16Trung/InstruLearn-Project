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

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO googleLoginDTO)
        {
            if (string.IsNullOrEmpty(googleLoginDTO.IdToken))
            {
                return BadRequest(new ResponseDTO { IsSucceed = false, Message = "Google ID token is required" });
            }

            var result = await _authService.GoogleLoginAsync(googleLoginDTO);
            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDTO);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(resetPasswordDTO);
            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyEmailAsync(verifyEmailDTO);
            if (!result.IsSucceed)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResendVerificationEmailAsync(model.Email);
            return Ok(result); // Always return OK for security reasons
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
                    .Select(a => new { a.AdminId, a.AccountId, a.Fullname, a.Account.Email, a.Account.Username, Role = role })
                    .FirstOrDefaultAsync(),

                "Staff" => await _dbContext.Staffs
                    .Where(s => s.AccountId == accountId)
                    .Select(s => new { s.StaffId, s.AccountId, s.Fullname, s.Account.Username, s.Account.Email, s.Account.PhoneNumber, s.Account.Gender, s.Account.Address, s.Account.Avatar, s.Account.DateOfEmployment, Role = role })
                    .FirstOrDefaultAsync(),

                "Teacher" => await _dbContext.Teachers
                    .Where(t => t.AccountId == accountId)
                    .Select(t => new { t.TeacherId, t.AccountId, t.Fullname, t.Account.Username, t.Account.Email, t.Account.Gender, t.Account.Address, t.Account.Avatar, t.Account.PhoneNumber, t.Account.DateOfEmployment, t.Heading, t.Details, t.Links, Role = role })
                    .FirstOrDefaultAsync(),

                "Manager" => await _dbContext.Managers
                    .Where(m => m.AccountId == accountId)
                    .Select(m => new { m.ManagerId, m.AccountId, m.Fullname, m.Account.Username, m.Account.Email, m.Account.Gender, m.Account.Address, m.Account.Avatar, m.Account.PhoneNumber, m.Account.DateOfEmployment, m.Account, Role = role })
                    .FirstOrDefaultAsync(),

                "Learner" => await _dbContext.Learners
                    .Where(l => l.AccountId == accountId)
                    .Select(l => new { l.LearnerId, l.AccountId, l.FullName, l.Account.Username, l.Account.Email, l.Account.Gender, l.Account.Address, l.Account.Avatar, l.Account.PhoneNumber, Role = role })
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
