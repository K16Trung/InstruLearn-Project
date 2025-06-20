﻿using InstruLearn_Application.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository.IRepository
{
    public interface IPurchaseRepository : IGenericRepository<Purchase>
    {
        Task<IEnumerable<Purchase>> GetByLearnerIdAsync(int learnerId);
    }
}
