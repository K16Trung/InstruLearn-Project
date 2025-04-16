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
using System.Net;

namespace InstruLearn_Application.BLL.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ILearnerRepository _learnerRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IEmailService _emailService;
        private readonly GoogleTokenValidator _googleTokenValidator;
        private IMapper _mapper;
        private JwtHelper _jwtHelper;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, IMapper mapper, JwtHelper jwtHelper, ILearnerRepository learnerRepository, IWalletRepository walletRepository, GoogleTokenValidator googleTokenValidator, IEmailService emailService)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
            _learnerRepository = learnerRepository;
            _walletRepository = walletRepository;
            _googleTokenValidator = googleTokenValidator;
            _emailService = emailService;
        }

        // Login
        public async Task<ResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            var response = new ResponseDTO();

            var user = await _authRepository.GetByUserName(loginDTO.Username);
            if (user == null)
            {
                response.Message = "Thông tin đăng nhập không hợp lệ";
                return response;
            }

            var isPasswordValid = VerifyPassword(loginDTO.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                response.Message = "Thông tin đăng nhập không hợp lệ";
                return response;
            }

            if (!user.IsEmailVerified)
            {
                response.Message = "Vui lòng xác minh email của bạn trước khi đăng nhập. Kiểm tra hộp thư đến để biết mã xác minh.";
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

            var existingEmail = await _authRepository.GetByEmail(registerDTO.Email);
            if (existingEmail != null)
            {
                response.Message = "Email đã tồn tại!";
                return response;
            }

            var account = _mapper.Map<Account>(registerDTO);

            account.AccountId = GenerateUniqueId();
            account.PasswordHash = HashPassword(registerDTO.Password);
            account.IsActive = AccountStatus.PendingEmailVerification;
            account.Role = AccountRoles.Learner;
            account.Token = string.Empty;
            account.RefreshToken = string.Empty;
            account.CreatedAt = DateTime.Now;
            account.IsEmailVerified = false;
            account.EmailVerificationToken = GenerateSixDigitCode();
            account.EmailVerificationTokenExpires = DateTime.Now.AddMinutes(2);

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

            await _emailService.SendVerificationEmailAsync(
                account.Email,
                account.Username,
                account.EmailVerificationToken
            );

            response.IsSucceed = true;
            response.Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác minh tài khoản của bạn.";
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
                    response.Message = "Mã thông báo Google không hợp lệ.";
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
                        CreatedAt = DateTime.Now,
                        IsEmailVerified = true,
                        PhoneNumber = string.Empty,
                        DateOfEmployment = new DateOnly(1900, 1, 1)
                    };

                    await _authRepository.AddAsync(account);

                    var learner = new Learner
                    {
                        AccountId = account.AccountId,
                        FullName = string.IsNullOrEmpty(googleLoginDTO.FullName) ? payload.Name ?? "Google User" : googleLoginDTO.FullName,
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
                response.Message = "Đăng nhập Google thành công!";
                response.Data = new { Token = token, RefreshToken = refreshToken };
            }
            catch (Exception ex)
            {
                // Log the full exception details including inner exception
                Console.WriteLine($"Google login error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                response.Message = $"Đăng nhập Google không thành công: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO)
        {
            var response = new ResponseDTO();

            var account = await _authRepository.GetByEmail(forgotPasswordDTO.Email);
            if (account == null)
            {

                response.IsSucceed = true;
                response.Message = "Nếu email của bạn đã được đăng ký với chúng tôi, bạn sẽ nhận được mã đặt lại mật khẩu.";
                return response;
            }

            var resetCode = GenerateSixDigitCode();

            account.RefreshToken = resetCode;
            account.RefreshTokenExpires = DateTime.Now.AddHours(1);
            await _authRepository.UpdateAsync(account);

            var subject = "Reset Your InstruLearn Password";
            var body = $@"
             <html>
               <body>
                 <h2>Yêu cầu đặt lại mật khẩu</h2>
                 <p>Xin chào {account.Username},</p>
                 <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu của bạn. Vui lòng sử dụng mã sau để đặt lại mật khẩu của bạn:</p>
                 <h3 style='font-size: 24px; background-color: #f5f5f5; padding: 10px; text-align: center;'>{resetCode}</h3>
                 <p>Mã này sẽ hết hạn sau 1 giờ.</p>
                 <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
                 <p>Trân trọng,</p>
                 <p>InstruLearn</p>
               </body>
             </html>";

            try
            {
                await _emailService.SendEmailAsync(forgotPasswordDTO.Email, subject, body);

                response.IsSucceed = true;
                response.Message = "Nếu email của bạn đã được đăng ký với chúng tôi, bạn sẽ nhận được mã đặt lại mật khẩu.";
            }
            catch (Exception ex)
            {
                response.Message = "Đã xảy ra lỗi khi gửi email đặt lại mật khẩu.";
            }

            return response;
        }

        public async Task<ResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            var response = new ResponseDTO();

            var account = await _authRepository.GetByEmail(resetPasswordDTO.Email);
            if (account == null)
            {
                response.Message = "Yêu cầu không hợp lệ.";
                return response;
            }

            if (account.RefreshToken != resetPasswordDTO.Token ||
                account.RefreshTokenExpires == null ||
                account.RefreshTokenExpires < DateTime.Now)
            {
                response.Message = "Mã thông báo không hợp lệ hoặc đã hết hạn.";
                return response;
            }

            account.PasswordHash = HashPassword(resetPasswordDTO.NewPassword);
            account.RefreshToken = string.Empty;
            account.RefreshTokenExpires = DateTime.MinValue;

            await _authRepository.UpdateAsync(account);

            response.IsSucceed = true;
            response.Message = "Mật khẩu đã được đặt lại thành công.";
            return response;
        }

        public async Task<ResponseDTO> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO)
        {
            var response = new ResponseDTO();

            var account = await _authRepository.GetByEmail(verifyEmailDTO.Email);
            if (account == null)
            {
                response.Message = "Yêu cầu xác minh không hợp lệ.";
                return response;
            }

            if (account.IsEmailVerified)
            {
                response.IsSucceed = true;
                response.Message = "Email đã được xác minh thành công.";
                return response;
            }

            if (account.EmailVerificationToken != verifyEmailDTO.Token ||
                account.EmailVerificationTokenExpires == null ||
                account.EmailVerificationTokenExpires < DateTime.Now)
            {
                response.Message = "Mã xác minh không hợp lệ hoặc đã hết hạn. Vui lòng đăng ký lại.";
                return response;
            }

            account.IsEmailVerified = true;
            account.EmailVerificationToken = null;
            account.EmailVerificationTokenExpires = null;
            account.IsActive = AccountStatus.Active;

            await _authRepository.UpdateAsync(account);

            response.IsSucceed = true;
            response.Message = "Email đã được xác minh thành công.";
            return response;
        }
        public async Task<ResponseDTO> ResendVerificationEmailAsync(string email)
        {
            var response = new ResponseDTO();

            var account = await _authRepository.GetByEmail(email);
            if (account == null)
            {

                response.IsSucceed = true;
                response.Message = "Nếu email của bạn đã được đăng ký với chúng tôi, bạn sẽ nhận được email xác minh.";
                return response;
            }

            if (account.IsEmailVerified)
            {
                response.IsSucceed = true;
                response.Message = "Email đã được xác minh.";
                return response;
            }

            account.EmailVerificationToken = GenerateSixDigitCode();
            account.EmailVerificationTokenExpires = DateTime.Now.AddHours(24);
            await _authRepository.UpdateAsync(account);

            await _emailService.SendVerificationEmailAsync(
                account.Email,
                account.Username,
                account.EmailVerificationToken
            );

            response.IsSucceed = true;
            response.Message = "Email xác minh đã được gửi.";
            return response;
        }
        private string GenerateSixDigitCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
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
