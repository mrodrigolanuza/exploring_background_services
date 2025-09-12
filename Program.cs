using exploring_background_services.Application;
using exploring_background_services.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Filters;

var builder = CreateApplicationBuilder(args);
ConfigureHostOptions(builder);
ConfigureWorkers(builder);
EnsureLogDirectories(builder.Configuration);
ConfigureLogging(builder);
var host = builder.Build();
ConfigureLifetimeEvents(host);
await RunHost(host);

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
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var workerA = services.GetRequiredService<IOptions<Worker_A_Settings>>().Value;
        var workerB = services.GetRequiredService<IOptions<Worker_B_Settings>>().Value;

        var workerAFile = Path.Combine(workerA.LogFilePath, "worker_A-.log");
        var workerBFile = Path.Combine(workerB.LogFilePath, "worker_B-.log");

        configuration.WriteTo.Console();

        configuration.WriteTo.Logger(lc =>
            lc.Filter.ByIncludingOnly(Matching.FromSource<Worker_A>())
              .WriteTo.File(workerAFile,
                            rollingInterval: ParseRollingInterval(workerA.RollingInterval),
                            restrictedToMinimumLevel: ConvertLogLevel(workerA.LogLevel)));

        configuration.WriteTo.Logger(lc =>
            lc.Filter.ByIncludingOnly(Matching.FromSource<Worker_B>())
              .WriteTo.File(workerBFile,
                            rollingInterval: ParseRollingInterval(workerB.RollingInterval),
                            restrictedToMinimumLevel: ConvertLogLevel(workerB.LogLevel)));
    });
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

void EnsureLogDirectories(IConfiguration configuration)
{
    var workerA = configuration.GetSection(nameof(Worker_A_Settings)).Get<Worker_A_Settings>();
    var workerB = configuration.GetSection(nameof(Worker_B_Settings)).Get<Worker_B_Settings>();

    foreach (var path in new[] { workerA?.LogFilePath, workerB?.LogFilePath })
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}

Serilog.Events.LogEventLevel ConvertLogLevel(LogLevel level) => level switch
{
    LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
    LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
    LogLevel.Information => Serilog.Events.LogEventLevel.Information,
    LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
    LogLevel.Error => Serilog.Events.LogEventLevel.Error,
    LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
    _ => Serilog.Events.LogEventLevel.Information
};

Serilog.RollingInterval ParseRollingInterval(string? interval) =>
    Enum.TryParse<Serilog.RollingInterval>(interval, true, out var result)
        ? result
        : Serilog.RollingInterval.Day;

