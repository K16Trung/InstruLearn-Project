using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class PaymentDeadlineService : BackgroundService
    {
        private readonly ILogger<PaymentDeadlineService> _logger;
        private readonly IServiceProvider _serviceProvider;

        // Check every hour
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public PaymentDeadlineService(
            ILogger<PaymentDeadlineService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Deadline Service started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPaymentDeadlinesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing payment deadlines");
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessPaymentDeadlinesAsync()
        {
            _logger.LogInformation("Checking for expired payment deadlines at: {time}", DateTimeOffset.Now);

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Get all learning registrations with status 'Accepted'
            var acceptedRegistrations = await unitOfWork.LearningRegisRepository.GetAcceptedRegistrationsAsync();
            int rejectedCount = 0;

            foreach (var registration in acceptedRegistrations)
            {
                // If payment deadline has passed and status is still 'Accepted'
                if (registration.PaymentDeadline.HasValue &&
                    DateTime.Now > registration.PaymentDeadline &&
                    registration.Status == LearningRegis.Accepted)
                {
                    _logger.LogInformation("Processing expired payment for registration ID: {id}, LearnerId: {learnerId}",
                        registration.LearningRegisId, registration.LearnerId);

                    try
                    {
                        using var transaction = await unitOfWork.BeginTransactionAsync();

                        // Update status to rejected
                        registration.Status = LearningRegis.Rejected;
                        registration.LearningRequest = "Automatically rejected due to non-payment within deadline.";

                        await unitOfWork.LearningRegisRepository.UpdateAsync(registration);

                        // Update associated test result if exists
                        var testResult = await unitOfWork.TestResultRepository
                            .GetByLearningRegisIdAsync(registration.LearningRegisId);

                        if (testResult != null)
                        {
                            testResult.Status = TestResultStatus.Cancelled;
                            await unitOfWork.TestResultRepository.UpdateAsync(testResult);
                        }

                        await unitOfWork.SaveChangeAsync();
                        await transaction.CommitAsync();

                        rejectedCount++;

                        _logger.LogInformation("Successfully auto-rejected registration ID: {id} due to missed payment deadline",
                            registration.LearningRegisId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to auto-reject registration ID: {id}", registration.LearningRegisId);
                    }
                }
            }

            _logger.LogInformation("Payment deadline check completed. Auto-rejected {count} registrations", rejectedCount);
        }
    }
}
