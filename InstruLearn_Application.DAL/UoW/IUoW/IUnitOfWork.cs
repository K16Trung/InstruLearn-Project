using InstruLearn_Application.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.DAL.UoW.IUoW
{
    public interface IUnitOfWork
    {
        ApplicationDbContext dbContext { get; }
        public Task<int> SaveChangeAsync();
    }
}
