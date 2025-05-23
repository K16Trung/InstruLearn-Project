using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using Microsoft.EntityFrameworkCore;
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
    public class ClassStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClassStatusBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public ClassStatusBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ClassStatusBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int checkIntervalHours = _configuration.GetValue<int>("ClassStatus:CheckIntervalHours", 24);

            _logger.LogInformation("Running initial class status check at startup");
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await UpdateAllClassStatusesAsync(unitOfWork);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var targetHour = _configuration.GetValue<int>("ClassStatus:CheckHour", 1);
                    var nextRun = now.Hour >= targetHour
                        ? now.Date.AddDays(1).AddHours(targetHour)
                        : now.Date.AddHours(targetHour);

                    var delay = nextRun - now;

                    _logger.LogInformation("Class status check scheduled for: {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        await UpdateAllClassStatusesAsync(unitOfWork);
                    }

                    await Task.Delay(TimeSpan.FromHours(checkIntervalHours), stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error occurred while updating class statuses");
                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
            }
        }

        private async Task UpdateAllClassStatusesAsync(IUnitOfWork unitOfWork)
        {
            try
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                var classes = await unitOfWork.ClassRepository.GetAllAsync();
                var updatedClasses = 0;

                foreach (var classEntity in classes)
                {
                    // Replace the non-existent GetByClassIdAsync method with GetQuery and filter
                    var classDays = await unitOfWork.ClassDayRepository.GetQuery()
                        .Where(cd => cd.ClassId == classEntity.ClassId)
                        .ToListAsync();

                    if (classDays == null || !classDays.Any())
                        continue;

                    var endDate = DateTimeHelper.CalculateEndDate(
                        classEntity.StartDate,
                        classEntity.totalDays,
                        classDays.Select(cd => cd.Day).ToList());

                    ClassStatus newStatus;
                    ClassStatus oldStatus = classEntity.Status;

                    if (classEntity.TestDay == today)
                    {
                        newStatus = ClassStatus.OnTestDay;
                    }
                    else if (classEntity.StartDate > today)
                    {
                        newStatus = ClassStatus.Scheduled;
                    }
                    else if (classEntity.StartDate <= today && endDate >= today)
                    {
                        newStatus = ClassStatus.Ongoing;
                    }
                    else
                    {
                        newStatus = ClassStatus.Completed;
                    }

                    if (oldStatus != newStatus)
                    {
                        classEntity.Status = newStatus;
                        await unitOfWork.ClassRepository.UpdateAsync(classEntity);
                        updatedClasses++;

                        _logger.LogInformation("Updated class {ClassId} status from {OldStatus} to {NewStatus}",
                            classEntity.ClassId, oldStatus, newStatus);
                    }
                }

                if (updatedClasses > 0)
                {
                    await unitOfWork.SaveChangeAsync();
                    _logger.LogInformation("Updated status for {Count} classes", updatedClasses);
                }
                else
                {
                    _logger.LogInformation("No class status updates needed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class statuses");
            }
        }
    }
}
