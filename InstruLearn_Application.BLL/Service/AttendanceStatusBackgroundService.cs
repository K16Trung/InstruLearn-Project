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
    public class AttendanceStatusBackgroundService : BackgroundService
    {
        private readonly ILogger<AttendanceStatusBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromHours(4); // Run every 4 hours

        public AttendanceStatusBackgroundService(
            ILogger<AttendanceStatusBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Attendance Status Background Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running attendance status auto-update job");

                try
                {
                    // Create a scope to resolve the schedule service
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
                        var result = await scheduleService.AutoUpdateAttendanceStatusAsync();

                        if (result.IsSucceed)
                        {
                            _logger.LogInformation("Successfully ran attendance auto-update: {Message}", result.Message);
                        }
                        else
                        {
                            _logger.LogWarning("Attendance auto-update completed with warnings: {Message}", result.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing attendance status auto-update");
                }

                // Wait for the next interval
                _logger.LogInformation("Attendance Status Background Service is waiting for the next interval");
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Attendance Status Background Service is stopping");
        }
    }
}
