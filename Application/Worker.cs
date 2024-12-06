using exploring_background_services.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exploring_background_services.Application
{
    /*
     * OBJETIVOS:
     * -[]Leer fichero config
     * -[]Escribir a fichero (Log)
     * -[]Leer de bbdd
     * -[]Enviar a App.Insigts
     */
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly WorkerSettings workerSettings;

        public Worker(ILogger<Worker> logger, IOptions<WorkerSettings> options)
        {
            this.logger = logger;
            workerSettings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                await Task.Delay(workerSettings.AutoExecutionDelay, stoppingToken);
            }
        }
    }
}
