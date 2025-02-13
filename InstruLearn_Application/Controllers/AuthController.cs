using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstruLearn_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
    }
}
