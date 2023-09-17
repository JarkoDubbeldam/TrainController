using System;
using Microsoft.Extensions.DependencyInjection;

namespace Z21;
public static class ServiceCollectionExtensions {
  public static IServiceCollection AddZ21(this IServiceCollection services, Action<Z21Settings> configureOptions) =>
    services
      .Configure<Z21Settings>(configureOptions)
      .AddTransient<IUdpClient, UdpClient>()
      .AddTransient<Func<IUdpClient>>(s => () => s.GetRequiredService<IUdpClient>())
      .AddSingleton<IZ21Client, Z21Client>();
}
