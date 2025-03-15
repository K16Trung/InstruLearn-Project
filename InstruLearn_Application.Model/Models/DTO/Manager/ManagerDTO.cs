using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Manager
{
    public class ManagerDTO
    {
        public int ManagerId { get; set; }
        public string AccountId { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public AccountStatus IsActive { get; set; }
    }
}
