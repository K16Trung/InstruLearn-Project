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
    public class CertificationRepository : GenericRepository<Certification>, ICertificationRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public CertificationRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Certification>> GetAllWithDetailsAsync()
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                .ToListAsync();
        }

        public async Task<Certification> GetByIdWithDetailsAsync(int id)
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                .FirstOrDefaultAsync(c => c.CertificationId == id);
        }

        public async Task<IEnumerable<Certification>> GetByLearnerIdAsync(int learnerId)
        {
            return await _appDbContext.Certifications
                .Include(c => c.Learner)
                .Where(c => c.LearnerId == learnerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Certification>> GetByLearningRegisIdAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _appDbContext.Learning_Registrations
                    .Include(lr => lr.Major)
                    .Include(lr => lr.Teacher)
                    .FirstOrDefaultAsync(lr => lr.LearningRegisId == learningRegisId);

                if (learningRegis == null)
                    return new List<Certification>();

                var teacherName = learningRegis.Teacher?.Fullname;
                var subjectName = learningRegis.Major?.MajorName;
                var learnerId = learningRegis.LearnerId;

                if (string.IsNullOrEmpty(teacherName) || string.IsNullOrEmpty(subjectName))
                    return new List<Certification>();

                return await _appDbContext.Certifications
                    .Include(c => c.Learner)
                    .Where(c => c.LearnerId == learnerId &&
                                c.TeacherName == teacherName &&
                                c.Subject == subjectName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByLearningRegisIdAsync: {ex.Message}");
                return new List<Certification>();
            }
        }

        public async Task<bool> ExistsByLearningRegisIdAsync(int learningRegisId)
        {
            try
            {
                var learningRegis = await _appDbContext.Learning_Registrations
                    .Include(lr => lr.Major)
                    .Include(lr => lr.Teacher)
                    .FirstOrDefaultAsync(lr => lr.LearningRegisId == learningRegisId);

                if (learningRegis == null)
                    return false;

                var teacherName = learningRegis.Teacher?.Fullname;
                var subjectName = learningRegis.Major?.MajorName;
                var learnerId = learningRegis.LearnerId;

                if (string.IsNullOrEmpty(teacherName) || string.IsNullOrEmpty(subjectName))
                    return false;

                return await _appDbContext.Certifications.AnyAsync(c =>
                    c.LearnerId == learnerId &&
                    c.TeacherName == teacherName &&
                    c.Subject == subjectName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExistsByLearningRegisIdAsync: {ex.Message}");
                return false;
            }
        }
    }
}