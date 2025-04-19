using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.Repository
{
    public class LearnerContentProgressRepository : GenericRepository<Learner_Content_Progress>, ILearnerContentProgressRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public LearnerContentProgressRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Learner_Content_Progress> GetByLearnerAndContentItemAsync(int learnerId, int itemId)
        {
            return await _appDbContext.LearnerContentProgresses
                .FirstOrDefaultAsync(lcp => lcp.LearnerId == learnerId && lcp.ItemId == itemId);
        }

        public async Task<List<Learner_Content_Progress>> GetByLearnerAndCourseAsync(int learnerId, int coursePackageId)
        {
            return await _appDbContext.LearnerContentProgresses
                .Include(lcp => lcp.ContentItem)
                    .ThenInclude(ci => ci.CourseContent)
                .Where(lcp => lcp.LearnerId == learnerId &&
                       lcp.ContentItem.CourseContent.CoursePackageId == coursePackageId)
                .ToListAsync();
        }

        public async Task<bool> UpdateWatchTimeAsync(int learnerId, int itemId, double watchTimeInSeconds, bool isCompleted = false)
        {
            var progress = await GetByLearnerAndContentItemAsync(learnerId, itemId);

            if (progress == null)
            {
                progress = new Learner_Content_Progress
                {
                    LearnerId = learnerId,
                    ItemId = itemId,
                    WatchTimeInSeconds = watchTimeInSeconds,
                    IsCompleted = isCompleted,
                    LastAccessDate = DateTime.Now
                };

                await _appDbContext.LearnerContentProgresses.AddAsync(progress);
            }
            else
            {
                progress.WatchTimeInSeconds = watchTimeInSeconds;
                progress.IsCompleted = isCompleted;
                progress.LastAccessDate = DateTime.Now;

                _appDbContext.LearnerContentProgresses.Update(progress);
            }

            await _appDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<double> GetTotalWatchTimeForCourseAsync(int learnerId, int coursePackageId)
        {
            var progresses = await GetByLearnerAndCourseAsync(learnerId, coursePackageId);
            return progresses.Sum(p => p.WatchTimeInSeconds);
        }

        public async Task<double> GetTotalVideoDurationForCourseAsync(int coursePackageId)
        {
            var courseContents = await _appDbContext.Course_Contents
                .Where(cc => cc.CoursePackageId == coursePackageId)
                .ToListAsync();

            double totalDuration = 0;

            foreach (var content in courseContents)
            {
                var contentItems = await _appDbContext.Course_Content_Items
                    .Where(ci => ci.ContentId == content.ContentId)
                    .ToListAsync();

                totalDuration += contentItems
                    .Where(ci => ci.DurationInSeconds.HasValue)
                    .Sum(ci => ci.DurationInSeconds.Value);
            }

            return totalDuration;
        }
    }
}