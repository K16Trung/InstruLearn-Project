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
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly ApplicationDbContext _appDbContext;

        public PaymentRepository(ApplicationDbContext appDbContext) : base(appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _appDbContext.Payments
                .Include(p => p.WalletTransaction)
                .FirstOrDefaultAsync(p => p.WalletTransaction.TransactionId == transactionId);
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            _appDbContext.Payments.Update(payment);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
