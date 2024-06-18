using AS.Application.Dependencies;
using AS.Infrastructure;
using AS.Infrastructure.Dependencies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connection = configuration.GetConnectionString("SQLServerSW");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInjectionInfrastructure(configuration);
builder.Services.AddInjectionApplication(configuration);
builder.Services.AddSingleton<Utilidades>();
builder.Services.AddMapperApplication();

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:key"]!))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS", app =>
    {
        app.AllowAnyOrigin().AllowAnyOrigin().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CORS");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
