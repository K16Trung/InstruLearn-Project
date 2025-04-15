using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Enum
{
    public enum AccountStatus
    {
        Banned = 0,                    // Account is banned
        Active = 1,                    // Account is active and verified
        PendingEmailVerification = 2   // Waiting for email confirmation
    }
}