﻿using InstruLearn_Application.DAL.Repository.IRepository;
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
    public class StaffRepository : GenericRepository<Staff>, IStaffRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public StaffRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<IEnumerable<Staff>> GetAllAsync()
        {
            return await _appDbContext.Staffs
                .Include(s => s.Account)
                .ToListAsync();
        }
        public async Task<Staff> GetByIdAsync(int id)
        {
            return await _appDbContext.Staffs
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.StaffId == id);
        }
    }
}
