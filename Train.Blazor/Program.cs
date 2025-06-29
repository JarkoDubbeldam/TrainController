using System.Net;
using Trains.DataAccess;
using TrainController;
using Z21;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using Trains.Blazor;
using Trains.Blazor.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddMudServices();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddTrainControllers();

builder.Services.AddTrainContext();
builder.Services.AddZ21(c => c.Z21Endpoint = new IPEndPoint(IPAddress.Parse("192.168.0.111"), 21105));
var app = builder.Build();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope()) {
  var db = scope.ServiceProvider.GetRequiredService<TrainContext>();
  db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment()) {
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();
//app.MapFallbackToPage("/_Host");

app.Run();
