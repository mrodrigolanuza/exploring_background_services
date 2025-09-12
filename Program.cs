using exploring_background_services.Application;
using exploring_background_services.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Filters;

var builder = CreateHostBuilder(args);
ConfigureHostOptions(builder);
ObtainWorkerSettingsFromConfigFile(builder);
ConfigureWorkerLogs(builder);
AddWorkersToHost(builder);
var host = builder.Build();
ConfigureHostLifetimeEvents(host);
await RunHost(host);

HostApplicationBuilder CreateHostBuilder(string[] strings)
{
    return Host.CreateApplicationBuilder(strings);
}

void ObtainWorkerSettingsFromConfigFile(HostApplicationBuilder builder)
{
    //Configure the settings for Workers from the appsettings.json file
    builder.Services.Configure<Worker_A_Settings>(builder.Configuration.GetSection(nameof(Worker_A_Settings)));
    builder.Services.Configure<Worker_B_Settings>(builder.Configuration.GetSection(nameof(Worker_B_Settings)));
}

void ConfigureHostOptions(HostApplicationBuilder builder)
{
    builder.Services.Configure<HostOptions>(o =>
    {
        o.StartupTimeout = Timeout.InfiniteTimeSpan;
        o.ServicesStartConcurrently = false;
    });
}

void ConfigureWorkerLogs(HostApplicationBuilder builder)
{
    //Logger: Worker_A
    var workerA = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<Worker_A_Settings>>().Value;
    var workerALogger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Console(restrictedToMinimumLevel: ConvertLogLevel(workerA.LogLevel))   //Consola
        .WriteTo.File(                                                                  //Fichero
            Path.Combine(workerA.LogFilePath, "worker_A-.log"), 
            rollingInterval: ParseRollingInterval(workerA.RollingInterval),
            restrictedToMinimumLevel: ConvertLogLevel(workerA.LogLevel))
        .Filter.ByIncludingOnly(Matching.FromSource<Worker_A>())
        .CreateLogger();
    
    //Logger: Worker_B
    var workerB = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<Worker_B_Settings>>().Value;
    var workerBLogger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Console(restrictedToMinimumLevel: ConvertLogLevel(workerB.LogLevel))   //Consola
        .WriteTo.File(                                                                  //Fichero
            Path.Combine(workerB.LogFilePath, "worker_B-.log"), 
            rollingInterval: ParseRollingInterval(workerB.RollingInterval),
            restrictedToMinimumLevel: ConvertLogLevel(workerB.LogLevel))
        .Filter.ByIncludingOnly(Matching.FromSource<Worker_B>())
        .CreateLogger();
    
    builder.Logging
        .ClearProviders()
        .AddSerilog(workerALogger)
        .AddSerilog(workerBLogger);
}

void AddWorkersToHost(HostApplicationBuilder builder)
{
    builder.Services.AddHostedService<Worker_A>();
    builder.Services.AddHostedService<Worker_B>();
}

void ConfigureHostLifetimeEvents(IHost host)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() => logger.LogInformation("HOST STARTED: WORKERS RUNNING!"));
    lifetime.ApplicationStopped.Register(() => logger.LogInformation("HOST FINISHED: WORKERS STOPPED!"));
}

Task RunHost(IHost host) => host.RunAsync();

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

