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
using InstruLearn_Application.Model.Models.DTO.Account;
using System.Net;

namespace InstruLearn_Application.BLL.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ILearnerRepository _learnerRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly GoogleTokenValidator _googleTokenValidator;
        private IMapper _mapper;
        private JwtHelper _jwtHelper;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, IMapper mapper, JwtHelper jwtHelper, ILearnerRepository learnerRepository, IWalletRepository walletRepository, GoogleTokenValidator googleTokenValidator)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
            _learnerRepository = learnerRepository;
            _walletRepository = walletRepository;
            _googleTokenValidator = googleTokenValidator;
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
            response.Message = "Đăng nhập thành công!";
            response.Data = new { Token = token, RefreshToken = refreshToken };

            return response;
        }

        // Register
        public async Task<ResponseDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            var response = new ResponseDTO();

            var existingUserName = await _authRepository.GetByUserName(registerDTO.Username);
            if (existingUserName != null)
            {
                response.Message = "Người dùng đã tồn tại!";
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
        // Login google
        public async Task<ResponseDTO> GoogleLoginAsync(GoogleLoginDTO googleLoginDTO)
        {
            var response = new ResponseDTO();

            try
            {
                var payload = await _googleTokenValidator.ValidateGoogleTokenAsync(googleLoginDTO.IdToken);

                if (payload == null)
                {
                    response.Message = "Invalid Google token.";
                    return response;
                }

                var email = payload.Email;

                var existingAccount = await _authRepository.GetByEmail(email);

                if (existingAccount == null)
                {
                    var account = new Account
                    {
                        AccountId = GenerateUniqueId(),
                        Username = email.Split('@')[0],
                        Email = email,
                        PasswordHash = HashPassword(GenerateRandomPassword()),
                        IsActive = AccountStatus.Active,
                        Role = AccountRoles.Learner,
                        Avatar = payload.Picture,
                        Token = string.Empty,
                        RefreshToken = string.Empty,
                        CreatedAt = DateTime.Now
                    };

                    await _authRepository.AddAsync(account);

                    var learner = new Learner
                    {
                        AccountId = account.AccountId,
                        FullName = payload.Name ?? "Google User",
                    };

                    await _learnerRepository.AddAsync(learner);

                    var wallet = new Wallet
                    {
                        LearnerId = learner.LearnerId,
                        Balance = 0,
                        UpdateAt = DateTime.UtcNow
                    };

                    await _walletRepository.AddAsync(wallet);

                    existingAccount = account;
                }

                var token = _jwtHelper.GenerateJwtToken(existingAccount);
                var refreshToken = _jwtHelper.GenerateRefreshToken();

                var tokenExpiration = DateTime.Now.AddHours(1);
                var refreshTokenExpiration = DateTime.Now.AddDays(7);

                existingAccount.Token = token;
                existingAccount.TokenExpires = tokenExpiration;
                existingAccount.RefreshToken = refreshToken;
                existingAccount.RefreshTokenExpires = refreshTokenExpiration;
                await _authRepository.UpdateAsync(existingAccount);

                response.IsSucceed = true;
                response.Message = "Google login successful!";
                response.Data = new { Token = token, RefreshToken = refreshToken };
            }
            catch (Exception ex)
            {
                response.Message = $"Google login failed: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO)
        {
            var response = new ResponseDTO();

            var account = await _authRepository.GetByEmail(forgotPasswordDTO.Email);
            if (account == null)
            {
                // For security, don't reveal that the email doesn't exist
                response.IsSucceed = true;
                response.Message = "If your email is registered with us, you will receive a password reset link.";
                return response;
            }

            // Generate a reset token
            var token = GenerateRandomPassword();

            // Store token in the account
            account.RefreshToken = token;
            account.RefreshTokenExpires = DateTime.Now.AddHours(1); // Token valid for 1 hour
            await _authRepository.UpdateAsync(account);

            // Generate the reset link
            var frontendUrl = _configuration["ApplicationSettings:FrontendUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(forgotPasswordDTO.Email)}";

            // Create the email body
            var subject = "Reset Your InstruLearn Password";
            var body = $@"
             <html>
                <body>
                  <h2>Password Reset Request</h2>
                  <p>Hello {account.Username},</p>
                  <p>We received a request to reset your password. Please click the link below to reset your password:</p>
                  <p><a href='{resetLink}'>Reset Password</a></p>
                  <p>This link will expire in 1 hour.</p>
                  <p>If you didn't request this, please ignore this email.</p>
                  <p>Best regards,</p>
                  <p>The InstruLearn Team</p>
                 </body>
             </html>";

            try
            {
                var emailService = new EmailService(_configuration);
                await emailService.SendEmailAsync(forgotPasswordDTO.Email, subject, body);

                response.IsSucceed = true;
                response.Message = "If your email is registered with us, you will receive a password reset link.";
            }
            catch (Exception ex)
            {
                response.Message = "There was an error sending the password reset email.";
            }

            return response;
        }

        public async Task<ResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            var response = new ResponseDTO();

            var account = await _authRepository.GetByEmail(resetPasswordDTO.Email);
            if (account == null)
            {
                response.Message = "Invalid request.";
                return response;
            }

            // Validate token
            if (account.RefreshToken != resetPasswordDTO.Token ||
                account.RefreshTokenExpires == null ||
                account.RefreshTokenExpires < DateTime.Now)
            {
                response.Message = "Invalid or expired token.";
                return response;
            }

            // Reset the password
            account.PasswordHash = HashPassword(resetPasswordDTO.NewPassword);
            account.RefreshToken = string.Empty;
            account.RefreshTokenExpires = DateTime.MinValue;

            await _authRepository.UpdateAsync(account);

            response.IsSucceed = true;
            response.Message = "Password has been reset successfully.";
            return response;
        }

        private string GenerateRandomPassword()
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+";
            var random = new Random();
            var password = new char[16];

            for (int i = 0; i < password.Length; i++)
            {
                password[i] = allowedChars[random.Next(allowedChars.Length)];
            }

            return new string(password);
        }
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); 
        }

        private bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, hashedPassword);
        }

        private string GenerateUniqueId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
