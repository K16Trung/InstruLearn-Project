using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Auth
{
    public class GoogleLoginDTO
    {
        public string IdToken { get; set; }
        public string FullName { get; set; }
    }
}