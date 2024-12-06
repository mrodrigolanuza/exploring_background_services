using exploring_background_services.Configuration;
using Microsoft.Extensions.Options;

namespace exploring_background_services.Application
{
    /*
     * OBJETIVOS:
     * -[]Leer fichero config
     * -[]Escribir a fichero (Log)
     * -[]Leer de bbdd
     * -[]Enviar a App.Insigts
     */
    public class Worker_A : BackgroundService
    {
        private readonly ILogger<Worker_A> logger;
        private readonly Worker_A_Settings workerSettings;

        public Worker_A(ILogger<Worker_A> logger, IOptions<Worker_A_Settings> options)
        {
            this.logger = logger;
            workerSettings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug("Worker A running at: {time}", DateTimeOffset.Now);
                await Task.Delay(workerSettings.AutoExecutionDelay, stoppingToken);
            }
        }
    }
}
