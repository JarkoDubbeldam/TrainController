using System;
using System.Threading.Tasks;

using Autofac;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

using Splat;

namespace TrainUI {
  class Program {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static async Task Main(string[] args) {
      try {
        BuildAvaloniaApp()
          .StartWithClassicDesktopLifetime(args);
      } finally {
        var scope = Locator.Current.GetService<ILifetimeScope>();
        await scope.DisposeAsync();
      }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseAutofac<AppBuilder, TrainUIModule>();
  }
}
