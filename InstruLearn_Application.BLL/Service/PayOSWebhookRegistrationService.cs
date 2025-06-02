using InstruLearn_Application.Model.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Net.payOS;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class PayOSWebhookRegistrationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PayOSWebhookRegistrationService> _logger;
        private readonly PayOSSettings _payOSSettings;

        public PayOSWebhookRegistrationService(
            IServiceProvider serviceProvider,
            IOptions<PayOSSettings> payOSSettings,
            ILogger<PayOSWebhookRegistrationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _payOSSettings = payOSSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            try
            {
                _logger.LogInformation("Attempting to register PayOS webhook...");

                PayOS payOS = new PayOS(
                    _payOSSettings.ClientId,
                    _payOSSettings.ApiKey,
                    _payOSSettings.ChecksumKey
                );

                string webhookUrl = "https://instrulearnapplication2025-h7hfdte3etdth7av.southeastasia-01.azurewebsites.net/api/payos/webhook";

                try
                {
                    var result = await payOS.confirmWebhook(webhookUrl);
                    _logger.LogInformation($"PayOS webhook registered successfully: {webhookUrl}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not register webhook with PayOS API: {ex.Message}");
                    _logger.LogInformation("Will rely on URL redirection and transaction polling for payment updates");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PayOS webhook registration");
            }
        }
    }
}
