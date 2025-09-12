using exploring_background_services.Application;
using exploring_background_services.Configuration;

EnsureLogDirectory();
var builder = CreateApplicationBuilder(args);
ConfigureHostOptions(builder);
ConfigureLogging(builder);
ConfigureWorkers(builder);
var host = builder.Build();
ConfigureLifetimeEvents(host);
await RunHost(host);
void EnsureLogDirectory() => Directory.CreateDirectory("Logs");

HostApplicationBuilder CreateApplicationBuilder(string[] strings)
{
    return Host.CreateApplicationBuilder(strings);
}

void ConfigureHostOptions(HostApplicationBuilder builder)
{
    builder.Services.Configure<HostOptions>(o =>
    {
        o.StartupTimeout = Timeout.InfiniteTimeSpan;
        o.ServicesStartConcurrently = false;
    });
}

void ConfigureLogging(HostApplicationBuilder builder)
{
    builder.Logging.AddConsole();
}

void ConfigureWorkers(HostApplicationBuilder builder)
{
    builder.Services.Configure<Worker_A_Settings>(builder.Configuration.GetSection(nameof(Worker_A_Settings)));
    builder.Services.Configure<Worker_B_Settings>(builder.Configuration.GetSection(nameof(Worker_B_Settings)));

    builder.Services.AddHostedService<Worker_A>();
    builder.Services.AddHostedService<Worker_B>();
}

void ConfigureLifetimeEvents(IHost host)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() => logger.LogInformation("HOST INICADO!"));
    lifetime.ApplicationStopped.Register(() => logger.LogInformation("HOST FINALIZADO!"));
}

Task RunHost(IHost host) => host.RunAsync();

