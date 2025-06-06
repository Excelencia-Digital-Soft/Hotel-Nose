using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ApiObjetos.Data;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ApiObjetos.NotificacionesHub;
using System;
using ApiObjetos.Jobs;
using ApiObjetos.Auth;
using System.IO;
using ApiObjetos.Mapping;
using ApiObjetos.Interfaces;
using ApiObjetos.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app
           .SetIsOriginAllowed(_ => true)
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Registrar AutoMapper con todos los perfiles en el ensamblado
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddAuthorization();

// Register JwtService
builder.Services.AddSingleton<JwtService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Configure JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Add DbContext
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Cron Job
builder.Services.AddCronJob<MySchedulerJob>(options =>
{
    options.CronExpression = "20 8 * * *";
    options.TimeZone = TimeZoneInfo.Local;
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<ReservationMonitorService>(); // Add background service

builder.Services.AddSingleton<ReservationMonitorService>();
builder.Services.AddHostedService<ReservationMonitorService>();

builder.Services.AddScoped<IStatisticsService, StatisticsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
});

// Habilitar archivos estáticos (imágenes, CSS, JavaScript, etc.)
app.UseStaticFiles();
//app.UseHttpsRedirection();
app.UseCors("NuevaPolitica");
app.UseAuthentication();  // Enable JWT authentication
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/notifications"); // WebSocket endpoint

app.Run();