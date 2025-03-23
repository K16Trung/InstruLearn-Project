using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models.DTO.Auth;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.DAL.Repository;

namespace InstruLearn_Application.BLL.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ILearnerRepository _learnerRepository;
        private readonly IWalletRepository _walletRepository;
        private IMapper _mapper;
        private JwtHelper _jwtHelper;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, IMapper mapper, JwtHelper jwtHelper, ILearnerRepository learnerRepository, IWalletRepository walletRepository)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
            _learnerRepository = learnerRepository;
            _walletRepository = walletRepository;
        }

        // Login
        public async Task<ResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            var response = new ResponseDTO();

            var user = await _authRepository.GetByUserName(loginDTO.Username);
            if (user == null)
            {
                response.Message = "Invalid credentials";
                return response;
            }

            var isPasswordValid = VerifyPassword(loginDTO.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                response.Message = "Invalid credentials";
                return response;
            }

            var token = _jwtHelper.GenerateJwtToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            var tokenExpiration = DateTime.Now.AddHours(1);
            var refreshTokenExpiration = DateTime.Now.AddDays(7);

            user.Token = token;
            user.TokenExpires = tokenExpiration;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpires = refreshTokenExpiration;
            await _authRepository.UpdateAsync(user);

            response.IsSucceed = true;
            response.Message = "Login successful!";
            response.Data = new { Token = token, RefreshToken = refreshToken };

            return response;
        }

        // Register
        public async Task<ResponseDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            var response = new ResponseDTO();

            var existingUser = await _authRepository.GetByUserName(registerDTO.Username);
            if (existingUser != null)
            {
                response.Message = "User already exists!";
                return response;
            }

            var account = _mapper.Map<Account>(registerDTO);

            account.AccountId = GenerateUniqueId();
            account.PasswordHash = HashPassword(registerDTO.Password);
            account.IsActive = AccountStatus.Active;
            account.Role = AccountRoles.Learner;
            account.Token = string.Empty;
            account.RefreshToken = string.Empty;
            account.CreatedAt = DateTime.Now;

            await _authRepository.AddAsync(account);

            var user = new Learner
            {
                AccountId = account.AccountId,
                FullName = registerDTO.FullName,
            };

            // Add the customer to the database
            await _learnerRepository.AddAsync(user);

            var wallet = new Wallet
            {
                LearnerId = user.LearnerId,
                Balance = 0,
                UpdateAt = DateTime.UtcNow
            };

            await _walletRepository.AddAsync(wallet);

            response.IsSucceed = true;
            response.Message = "Registration successful!";
            response.Data = true;
            return response;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // Explicit namespace
        }

        private bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, hashedPassword); // Explicit namespace
        }

        private string GenerateUniqueId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
