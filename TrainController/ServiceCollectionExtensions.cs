using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrainController.Occupancies;
using TrainController.Segments;
using TrainController.Signals;
using TrainController.Turnouts;

namespace TrainController;
public static class ServiceCollectionExtensions {
  public static IServiceCollection AddTrainControllers(this IServiceCollection services) =>
    services.RegisterController<TurnoutController, Turnout>()
      .RegisterController<SignalController, Signal>()
      .RegisterController<OccupancyController, Occupancy>()
      .RegisterController<SegmentController, Segment>()
      .AddHostedService<SignalStateController>();

  private static IServiceCollection RegisterController<T, TObject>(this IServiceCollection services)
    where T : class, IController<TObject>, IHostedService
    where TObject : class =>
    services.AddSingleton<T>()
      .AddSingleton<IController<TObject>>(sp => sp.GetRequiredService<T>())
      .AddHostedService(sp => sp.GetRequiredService<T>());
}
