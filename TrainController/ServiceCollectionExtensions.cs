using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrainController.Turnouts;

namespace TrainController;
public static class ServiceCollectionExtensions {
  public static IServiceCollection AddControllers(this IServiceCollection services) =>
    services.RegisterController<TurnoutController, Turnout>();

  private static IServiceCollection RegisterController<T, TObject>(this IServiceCollection services)
    where T : class, IController<TObject>, IHostedService
    where TObject : class =>
    services.AddSingleton<T>()
      .AddSingleton<IController<TObject>>(sp => sp.GetRequiredService<T>())
      .AddHostedService(sp => sp.GetRequiredService<T>());
}
