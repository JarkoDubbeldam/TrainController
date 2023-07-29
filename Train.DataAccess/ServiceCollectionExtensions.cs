using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Trains.DataAccess.Services;

namespace Trains.DataAccess;
public static class ServiceCollectionExtensions {
  public static IServiceCollection AddTrainContext(this IServiceCollection services) =>
    services
      .AddDbContext<TrainContext>(db => db.UseSqlite("Data Source=train.db"))
      .AddTransient<ITrainService, TrainService>();
}
