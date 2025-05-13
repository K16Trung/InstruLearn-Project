using InstruLearn_Application.BLL.Service.IService;
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
    public class FeedbackNotificationBackgroundService : BackgroundService
    {
        private readonly ILogger<FeedbackNotificationBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Check once a day
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        public FeedbackNotificationBackgroundService(
            ILogger<FeedbackNotificationBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Feedback Notification Service started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running automatic feedback notification check at: {time}", DateTimeOffset.Now);

                    using var scope = _serviceScopeFactory.CreateScope();
                    var feedbackService = scope.ServiceProvider.GetRequiredService<IFeedbackNotificationService>();

                    var result = await feedbackService.AutoCheckAndCreateFeedbackNotificationsAsync();

                    if (result.IsSucceed)
                    {
                        _logger.LogInformation("Feedback notification check completed successfully: {message}", result.Message);
                    }
                    else
                    {
                        _logger.LogWarning("Feedback notification check completed with warnings: {message}", result.Message);
                    }

                    // Check for expired feedbacks and update their status
                    var expiredResult = await feedbackService.CheckForExpiredFeedbacksAsync();
                    if (expiredResult.IsSucceed)
                    {
                        _logger.LogInformation("Expired feedback check completed successfully: {message}", expiredResult.Message);
                    }
                    else
                    {
                        _logger.LogWarning("Expired feedback check completed with warnings: {message}", expiredResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for feedback notifications");
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Feedback Notification Service stopped at: {time}", DateTimeOffset.Now);
        }
    }
}
