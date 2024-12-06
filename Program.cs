using exploring_background_services.Application;
using exploring_background_services.Configuration;

//Host for para albergar tareas "long-running" de tipo Aplicaci�n.
var builder = Host.CreateApplicationBuilder(args);

//Opciones de configuraci�n del Host (HOstOptions)
//-Timeout de inicio de hosted servicies (infinito por defecto)
//-Timeout de finalizaci�n de hosted services (30 segundos por defecto) > Tiempo que tienen los hosted services cuando reciben la se�al (cancellation token) de finalizar "gracefully".
//-Iniciar hosted services en paralelo (por defecto secuencialmente)
//-Finalizar hosted services en paralelo (por defecto secuenicalmente)
//-Comportamiento ante una excepci�n en un hosted service (por defecto finalizar el Host completo)
builder.Services.Configure<HostOptions>(o =>
{
    o.StartupTimeout = Timeout.InfiniteTimeSpan;
    o.ServicesStartConcurrently = false;
});

//Opciones de configuraci�n para el worker mediante DI
builder.Services.Configure<Worker_A_Settings>(builder.Configuration.GetSection(key: nameof(Worker_A_Settings)));
builder.Services.Configure<Worker_B_Settings>(builder.Configuration.GetSection(key: nameof(Worker_B_Settings)));

//Registrar en el host los servicios hospedados a ejecutar (por defecto se ejecutan en seg�n el orden indicado al registrarlos)
builder.Services.AddHostedService<Worker_A>();
builder.Services.AddHostedService<Worker_B>();
var host = builder.Build();

//Zona para realizar tareas previas a la ejecuci�n (configuraciones, inicializaciones, acceso base de datos..)
//Para ello podemo usar el contenedor de dependencias out-of-the-box.
//Ejemplo de obtenci�n de un servicio: Registrar acciones relacionadas con eventos del ciclo de vida.
var hostAppLifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
hostAppLifeTime.ApplicationStarted.Register(() => Console.WriteLine("HOST INICADO!"));
hostAppLifeTime.ApplicationStopped.Register(() => Console.WriteLine("HOST FINALIZADO!"));

//Iniciar los Hosted Services (long-running tasks) que incluye.
await host.RunAsync();
