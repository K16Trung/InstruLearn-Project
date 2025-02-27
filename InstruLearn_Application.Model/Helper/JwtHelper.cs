using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Helper
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public JwtHelper(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public string GenerateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);


            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, account.AccountId),
                new Claim(JwtRegisteredClaimNames.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role.ToString()) // Role claim
            };

            // Single optimized DB query to fetch account with role details
            var accountWithDetails = _dbContext.Accounts
                .Include(a => a.Admin)
                .Include(a => a.Staff)
                .Include(a => a.Teacher)
                .Include(a => a.Manager)
                .Include(a => a.Learner)
                .FirstOrDefault(a => a.AccountId == account.AccountId);

            if (accountWithDetails?.Admin != null)
                claims.Add(new Claim("AdminId", accountWithDetails.Admin.AdminId.ToString()));
            else if (accountWithDetails?.Staff != null)
                claims.Add(new Claim("StaffId", accountWithDetails.Staff.StaffId.ToString()));
            else if (accountWithDetails?.Teacher != null)
                claims.Add(new Claim("TeacherId", accountWithDetails.Teacher.TeacherId.ToString()));
            else if (accountWithDetails?.Manager != null)
                claims.Add(new Claim("ManagerId", accountWithDetails.Manager.ManagerId.ToString()));
            else if (accountWithDetails?.Learner != null)
                claims.Add(new Claim("LearnerId", accountWithDetails.Learner.LearnerId.ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:validissuer"],
                Audience = _configuration["Jwt:validAudience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
