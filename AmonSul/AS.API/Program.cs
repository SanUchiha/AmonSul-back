using AS.API.Configurations;
using AS.API.Filters;
using AS.Application.Extensions;
using AS.Application.Interfaces;
using AS.Infrastructure.Extensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
IServiceCollection services = builder.Services;
string? connection = configuration.GetConnectionString("MyASPString");

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerConfigurations();
services.AddInfrastructureServices(configuration);
services.AddApplicationServices(configuration);
services.AddScoped<AdminTorneoFilter>();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("CORS");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard();

BackgroundJob.Enqueue(() => Console.WriteLine("Amon s�l configurada correctamente."));

// El trabajo antiguo guarda el tipo ILigaApplication, que ya no existe: se quita para que no falle
RecurringJob.RemoveIfExists("crear-liga-amon-sul-anual");
RecurringJob.AddOrUpdate<ICircuitoApplication>(
    "crear-circuito-amon-sul-anual",
    job => job.CrearCircuitoAmonSulAnualAsync(),
    "0 0 1 1 *",
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time")
    }
);

RecurringJob.AddOrUpdate<IEloApplication>(
    "actualizar-cache-clasificacion-elo",
    job => job.UpdateClasificacionEloCacheAsync(),
    "0 23 * * 0",
    new RecurringJobOptions 
    { 
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time") 
    }
);

// El último día de cada mes se guardan las posiciones de los torneos terminados que nadie ha guardado
RecurringJob.AddOrUpdate<IGanadorApplication>(
    "guardar-posiciones-torneos-terminados",
    job => job.GuardarPosicionesTorneosTerminadosAsync(),
    "0 23 L * *",
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time")
    }
);
// Los que ya estaban terminados se guardan al publicar esto, sin esperar a fin de mes. Repetirlo no hace nada:
// solo coge los que siguen sin posiciones
if (DateOnly.FromDateTime(DateTime.Now) <= new DateOnly(2026, 11, 3))
    RecurringJob.TriggerJob("guardar-posiciones-torneos-terminados");

app.Run();
