using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Trains.DataAccess.Services;

namespace Trains.DataAccess;
public static class ServiceCollectionExtensions {
  public static IServiceCollection AddTrainContext(this IServiceCollection services) =>
    services
      .AddDbContext<TrainContext>(db => db.UseSqlite("Data Source=train.db"), contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Transient)
      .AddTransient<ITrainService, TrainService>()
      .AddTransient<ITurnoutService, TurnoutService>()
      .AddTransient<ISignalService, SignalService>();
}
