using System;

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
    public static void Main(string[] args) {
      try {
        BuildAvaloniaApp()
          .StartWithClassicDesktopLifetime(args);
      } finally {
        var scope = Locator.Current.GetService<ILifetimeScope>();
        scope.Dispose();
      }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToDebug(Avalonia.Logging.LogEventLevel.Information)
            .UseAutofac<AppBuilder, TrainUIModule>();
  }
}
