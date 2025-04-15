using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IAuthService
    {
        Task<ResponseDTO> RegisterAsync(RegisterDTO registerDTO);
        Task<ResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task<ResponseDTO> GoogleLoginAsync(GoogleLoginDTO googleLoginDTO);
        Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDTO);
        Task<ResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
        Task<ResponseDTO> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO);
        Task<ResponseDTO> ResendVerificationEmailAsync(string email);
    }
}
