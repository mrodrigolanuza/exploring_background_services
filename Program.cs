using exploring_background_services.Application;
using exploring_background_services.Configuration;

//Host for para albergar tareas "long-running" de tipo Aplicación.
var builder = Host.CreateApplicationBuilder(args);

//Opciones de configuración del Host (HOstOptions)
//-Timeout de inicio de hosted servicies (infinito por defecto)
//-Timeout de finalización de hosted services (30 segundos por defecto) > Tiempo que tienen los hosted services cuando reciben la señal (cancellation token) de finalizar "gracefully".
//-Iniciar hosted services en paralelo (por defecto secuencialmente)
//-Finalizar hosted services en paralelo (por defecto secuenicalmente)
//-Comportamiento ante una excepción en un hosted service (por defecto finalizar el Host completo)
builder.Services.Configure<HostOptions>(o =>
{
    o.StartupTimeout = Timeout.InfiniteTimeSpan;
    o.ServicesStartConcurrently = false;
});

//Opciones de configuración para el worker mediante DI
builder.Services.Configure<WorkerSettings>(builder.Configuration.GetSection(key: nameof(WorkerSettings)));

//Registrar en el host los servicios hospedados a ejecutar (por defecto se ejecutan en según el orden indicado al registrarlos)
builder.Services.AddHostedService<Worker>();
var host = builder.Build();

//Zona para realizar tareas previas a la ejecución (configuraciones, inicializaciones, acceso base de datos..)
//Para ello podemo usar el contenedor de dependencias out-of-the-box.
//Ejemplo de obtención de un servicio: Registrar acciones relacionadas con eventos del ciclo de vida.
var hostAppLifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
hostAppLifeTime.ApplicationStarted.Register(() => Console.WriteLine("HOST INICADO!"));
hostAppLifeTime.ApplicationStopped.Register(() => Console.WriteLine("HOST FINALIZADO!"));

//Iniciar los Hosted Services (long-running tasks) que incluye.
await host.RunAsync();
