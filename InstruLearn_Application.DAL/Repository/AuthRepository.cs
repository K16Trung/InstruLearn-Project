using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class AuthRepository : GenericRepository<Account>, IAuthRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public AuthRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            {
                _appDbContext = appDbContext;
            }

        }

        public async Task<Account?> GetByUserName(string Username)
        {
            return await _appDbContext.Accounts.SingleOrDefaultAsync(u => u.Username == Username);
        }

        public async Task<Account> GetByEmail(string email)
        {
            return await _appDbContext.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }
    }
}
