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
    public class ResponseRepository : GenericRepository<Response>, IResponseRepository
    {
        private readonly ApplicationDbContext _appDbContext;
        public ResponseRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Response>> GetAllAsync()
        {
            return await _appDbContext.Responses
                .Include(r => r.ResponseType)
                .ToListAsync();
        }
        public async Task<Response> GetByIdAsync(int responseId)
        {
            return await _appDbContext.Responses
                .Include(r => r.ResponseType)
                .FirstOrDefaultAsync(r => r.ResponseId == responseId);
        }
    }
}