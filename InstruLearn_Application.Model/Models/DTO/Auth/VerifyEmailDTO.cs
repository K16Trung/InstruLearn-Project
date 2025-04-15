using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Auth
{
    public class VerifyEmailDTO
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must contain exactly 6 digits.")]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}