﻿using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class LearningRegisRepository : GenericRepository<Learning_Registration>, ILearningRegisRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public LearningRegisRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Learning_Registration>> GetPendingRegistrationsAsync()
        {
            return await _appDbContext.Learning_Registrations
                .Where(x => x.Status == LearningRegis.Pending)
                .Include(x => x.Learner)
                .Include(x => x.Teacher)
                .Include(l => l.Learner.Account)
                .Include(x => x.Learning_Registration_Type)
                .Include(x => x.Major)
                .Include(l => l.Response)
                .Include(l => l.Response.ResponseType)
                .Include(l => l.LevelAssigned)
                .Include(x => x.LearningRegistrationDay)
                .Include(x => x.SelfAssessment)
                .ToListAsync();
        }

        public async Task<IEnumerable<Learning_Registration>> GetRegistrationsByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.Learning_Registrations
                .Where(x => x.LearnerId == learnerId)
                .Include(x => x.Learner)
                .Include(x => x.Teacher)
                .Include(l => l.Learner.Account)
                .Include(x => x.Learning_Registration_Type)
                .Include(x => x.Major)
                .Include(l => l.Response)
                .Include(l => l.Response.ResponseType)
                .Include(l => l.LevelAssigned)
                .Include(x => x.LearningRegistrationDay)
                .Include(x => x.SelfAssessment)
                .ToListAsync();
        }

        public async Task<IEnumerable<Learning_Registration>> GetAllAsync()
        {
            return await _appDbContext.Learning_Registrations
                .Include(l => l.Teacher)
                .Include(l => l.Learner)
                .Include(l => l.Learner.Account)
                .Include(l => l.Learning_Registration_Type)
                .Include(l => l.Major)
                .Include(l => l.Response)
                .Include(l => l.Response.ResponseType)
                .Include(l => l.LevelAssigned)
                .Include(l => l.LearningRegistrationDay)
                .Include(x => x.SelfAssessment)
                .ToListAsync();
        }

        public async Task<Learning_Registration> GetByIdAsync(int id)
        {
            return await _appDbContext.Learning_Registrations
                .Include(l => l.Teacher)
                .Include(l => l.Learner)
                .Include(l => l.Learner.Account)
                .Include(l => l.Learning_Registration_Type)
                .Include(l => l.Major)
                .Include(l => l.Response)
                .Include(l => l.Response.ResponseType)
                .Include(l => l.LevelAssigned)
                .Include(l => l.LearningRegistrationDay)
                .Include(x => x.SelfAssessment)
                .FirstOrDefaultAsync(l => l.LearningRegisId == id);
        }

        public async Task<Learning_Registration?> GetFirstOrDefaultAsync(
            Expression<Func<Learning_Registration, bool>> predicate,
            Func<IQueryable<Learning_Registration>, IIncludableQueryable<Learning_Registration, object>>? include = null)
        {
            IQueryable<Learning_Registration> query = _appDbContext.Learning_Registrations;

            if (include != null)
                query = include(query);
            else
                query = query.Include(lr => lr.LearningRegistrationDay);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<Learning_Registration>> GetAcceptedRegistrationsAsync()
        {
            return await _appDbContext.Learning_Registrations
                .Where(x => x.Status == LearningRegis.Accepted && x.PaymentDeadline.HasValue)
                .Include(x => x.Learner)
                .Include(x => x.Teacher)
                .Include(l => l.Learner.Account)
                .Include(x => x.Learning_Registration_Type)
                .Include(x => x.Major)
                .Include(x => x.SelfAssessment)
                .ToListAsync();
        }

        public async Task<IEnumerable<Learning_Registration>> GetRegistrationsByTeacherIdAsync(int teacherId)
        {
            return await _appDbContext.Learning_Registrations
                .Where(x => x.TeacherId == teacherId)
                .Include(x => x.Learner)
                .Include(x => x.Teacher)
                .Include(l => l.Learner.Account)
                .Include(x => x.Learning_Registration_Type)
                .Include(x => x.Major)
                .Include(l => l.Response)
                .Include(l => l.Response.ResponseType)
                .Include(l => l.LevelAssigned)
                .Include(x => x.LearningRegistrationDay)
                .Include(x => x.SelfAssessment)
                .ToListAsync();
        }

    }
}
