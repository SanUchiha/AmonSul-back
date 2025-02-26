using AS.API.Filters;
using AS.Application.Extensions;
using AS.Infrastructure.Extensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
IServiceCollection services = builder.Services;
string? connection = configuration.GetConnectionString("MyASPString");

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
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

BackgroundJob.Enqueue(() => Console.WriteLine("Amon sûl configurada correctamente."));

app.Run();
