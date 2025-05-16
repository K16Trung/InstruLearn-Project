using InstruLearn_Application.BLL.Service.IService;
using Microsoft.Extensions.Configuration;
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
    public class TeacherEvaluationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TeacherEvaluationBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public TeacherEvaluationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<TeacherEvaluationBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int checkIntervalHours = _configuration.GetValue<int>("TeacherEvaluation:CheckIntervalHours", 24);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var targetHour = _configuration.GetValue<int>("TeacherEvaluation:CheckHour", 8);
                    var nextRun = now.Hour >= targetHour
                        ? now.Date.AddDays(1).AddHours(targetHour)
                        : now.Date.AddHours(targetHour);

                    var delay = nextRun - now;

                    _logger.LogInformation("Teacher evaluation check scheduled for: {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var evaluationService = scope.ServiceProvider.GetRequiredService<ITeacherEvaluationService>();
                        var result = await evaluationService.CheckForLastDayEvaluationsAsync();

                        if (result.IsSucceed && result.Data != null)
                        {
                            _logger.LogInformation("Created evaluations for learners on their last day: {CreatedCount}",
                                ((System.Text.Json.JsonElement)result.Data).EnumerateArray().Count());
                        }
                        else
                        {
                            _logger.LogInformation("No evaluations created for last day learners: {Message}", result.Message);
                        }
                    }

                    await Task.Delay(TimeSpan.FromHours(checkIntervalHours), stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error occurred while checking for last day learner evaluations");
                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
            }
        }
    }
}
