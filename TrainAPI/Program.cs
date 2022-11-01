using System.Net;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrainAPI.Data;
using TrainAPI.Models;
using TrainAPI.Trackers;
using Z21;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TrainAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TrainAPIContext") ?? throw new InvalidOperationException("Connection string 'TrainAPIContext' not found.")));

// Add services to the container.

var ipAdress = new IPAddress(new byte[] { 192, 168, 0, 111 });
var endpoint = new IPEndPoint(ipAdress, 21105);
builder.Services.AddTransient<IZ21Client, NewZ21Client>(s => new NewZ21Client(endpoint));

builder.Services.AddControllers().AddJsonOptions(opts => {
  var enumConverter = new JsonStringEnumConverter();
  opts.JsonSerializerOptions.Converters.Add(enumConverter);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITurnoutTracker, TurnoutTracker>();
builder.Services.AddHostedService(s => s.GetRequiredService<ITurnoutTracker>() as TurnoutTracker);
builder.Services.AddSingleton<ITrainTracker, TrainTracker>();
builder.Services.AddHostedService(s => s.GetRequiredService<ITrainTracker>() as TrainTracker);
builder.Services.AddSingleton<IOccupancyTracker, OccupancyTracker>();
builder.Services.AddHostedService(s => s.GetRequiredService<IOccupancyTracker>() as OccupancyTracker);
builder.Services.AddSingleton<ITrackProvider, TrackProvider>();
builder.Services.AddSingleton<ITrainPositionTracker, TrainPositionTracker>();
builder.Services.AddHostedService(s => s.GetRequiredService<ITrainPositionTracker>() as TrainPositionTracker);
builder.Services.AddSingleton<ISectionLocker, SectionLocker>();
builder.Services.AddTransient<INavigator, Navigator>();

builder.Host.UseConsoleLifetime();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
