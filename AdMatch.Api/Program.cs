using AdMatch.Application.Services;
using AdMatch.DataAccessInterfaces.Repositories;
using AdMatch.Domain.Repositories;
using AdMatch.MicroserviceInterfaces.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAdvertisingService, AdvertisingService>();
builder.Services.AddSingleton<IAdvertisingRepository, AdvertisingRepository>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
