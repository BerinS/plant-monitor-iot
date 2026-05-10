using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Prevent MQTT background service failure from taking down entire app
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddHostedService<MqttBackgroundService>();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngularApp");
app.UseAuthorization();
app.MapControllers();
app.Run();