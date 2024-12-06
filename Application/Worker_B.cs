using exploring_background_services.Configuration;
using Microsoft.Extensions.Options;

namespace exploring_background_services.Application
{
    public class Worker_B : BackgroundService
    {
        private readonly ILogger<Worker_B> logger;
        private readonly Worker_B_Settings workerSettings;

        public Worker_B(ILogger<Worker_B> logger, IOptions<Worker_B_Settings> options)
        {
            this.logger = logger;
            workerSettings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("Worker B running at: {time}", DateTimeOffset.Now);
                await Task.Delay(workerSettings.AutoExecutionDelay, stoppingToken);
            }
        }
    }
}
