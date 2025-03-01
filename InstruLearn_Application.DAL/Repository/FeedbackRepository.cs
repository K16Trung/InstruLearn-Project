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
    public class FeedbackRepository : GenericRepository<FeedBack>, IFeedbackRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public FeedbackRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<IEnumerable<FeedBack>> GetAllAsync()
        {
            return await _appDbContext.FeedBacks
                .Include(f => f.Account)
                .Include(f => f.FeedbackReplies)
                    .ThenInclude(r => r.Account)
                .ToListAsync();
        }
        public async Task<FeedBack> GetByIdAsync(int id)
        {
            return await _appDbContext.FeedBacks
                .Include(f => f.Account)
                .Include(f => f.FeedbackReplies)
                    .ThenInclude(r => r.Account)
                .FirstOrDefaultAsync(f => f.FeedbackId == id);
        }
    }
}
