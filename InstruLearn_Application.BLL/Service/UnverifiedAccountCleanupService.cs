using InstruLearn_Application.DAL.Repository.IRepository;
using InstruLearn_Application.Model.Data;
using InstruLearn_Application.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class UnverifiedAccountCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnverifiedAccountCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

        public UnverifiedAccountCleanupService(
            IServiceProvider serviceProvider,
            ILogger<UnverifiedAccountCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Unverified Account Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredUnverifiedAccounts();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up unverified accounts.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Unverified Account Cleanup Service is stopping.");
        }

        private async Task CleanupExpiredUnverifiedAccounts()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.Now;
            var expiredAccounts = await dbContext.Accounts
                .Where(a => a.IsEmailVerified == false &&
                       a.IsActive == AccountStatus.PendingEmailVerification &&
                       a.EmailVerificationTokenExpires < now)
                .ToListAsync();

            if (expiredAccounts.Any())
            {
                _logger.LogInformation($"Deleting {expiredAccounts.Count} expired unverified accounts");

                var accountIds = expiredAccounts.Select(a => a.AccountId).ToList();
                var learners = await dbContext.Learners
                    .Where(l => accountIds.Contains(l.AccountId))
                    .ToListAsync();

                var learnerIds = learners.Select(l => l.LearnerId).ToList();
                var wallets = await dbContext.Wallets
                    .Where(w => learnerIds.Contains(w.LearnerId))
                    .ToListAsync();

                if (wallets.Any())
                {
                    dbContext.Wallets.RemoveRange(wallets);
                }

                if (learners.Any())
                {
                    dbContext.Learners.RemoveRange(learners);
                }

                dbContext.Accounts.RemoveRange(expiredAccounts);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Successfully deleted {expiredAccounts.Count} expired unverified accounts");
            }
        }
    }
}