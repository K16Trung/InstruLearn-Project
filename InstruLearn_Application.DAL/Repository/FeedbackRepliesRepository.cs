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
    public class FeedbackRepliesRepository : GenericRepository<FeedbackReplies>, IFeedbackRepliesRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public FeedbackRepliesRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<IEnumerable<FeedbackReplies>> GetAllAsync()
        {
            return await _appDbContext.FeedbackReplies
                .Include(f => f.Account)
                .ToListAsync();
        }
        public async Task<FeedbackReplies> GetByIdAsync(int feedbackRepliesId)
        {
            return await _appDbContext.FeedbackReplies
                .Include(f => f.Account)
                .FirstOrDefaultAsync(f => f.FeedbackRepliesId == feedbackRepliesId);
        }
    }
}
