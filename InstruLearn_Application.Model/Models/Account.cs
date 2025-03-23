using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Account
    {
        [Key]
        public string AccountId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public DateOnly DateOfEmployment { get; set; }
        public AccountRoles Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public AccountStatus IsActive { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpires { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpires { get; set; }

        // Navigation Properties
        public virtual Admin? Admin { get; set; }
        public virtual Staff? Staff { get; set; }
        public virtual Teacher? Teacher { get; set; }
        public virtual Manager? Manager { get; set; }
        public virtual Learner? Learner { get; set; }
        public ICollection<FeedBack> FeedBacks { get; set; }
        public ICollection<QnA> QnAs { get; set; }
        public ICollection<FeedbackReplies> FeedbackReplies { get; set; }
        public ICollection<QnAReplies> QnAReplies { get; set; }


    }
}
