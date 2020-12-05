using System;
using System.Collections.Generic;
using System.Net;

using Autofac;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using Newtonsoft.Json;

using ReactiveUI;

using Splat;

using TrainRepository;

using TrainUI.Converters;
using TrainUI.ViewModels;
using TrainUI.Views;

namespace TrainUI {
  public class App : Application {

    public override void Initialize() {
      AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
      //var suspension = new AutoSuspendHelper(ApplicationLifetime);
      //RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
      //var suspensionSettings = new JsonSuspensionSettings {
      //  Filename = "appstate.json"
      //};
      //RxApp.SuspensionHost.SetupDefaultSuspendResume(container.Resolve<Func<JsonSuspensionSettings, JsonSuspensionDriver>>()(suspensionSettings));


      //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      //  var context = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();
      //  //context ??= factory(new TrainModel { Name = "Db loc", Address = 3 });
      //  desktop.MainWindow = new MainWindow {
      //    DataContext = context,
      //  };
      //}

      //base.OnFrameworkInitializationCompleted();

      // Create the AutoSuspendHelper.
      var suspension = new AutoSuspendHelper(ApplicationLifetime);
      RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
      RxApp.SuspensionHost.SetupDefaultSuspendResume(new JsonSuspensionDriver(new JsonSuspensionSettings { 
        Filename = "appstate.json", 
        JsonSerializerSettings = new JsonSerializerSettings {
          TypeNameHandling = TypeNameHandling.All,
          Formatting = Formatting.Indented
        }
      }, Locator.Current.GetService<IList<JsonConverter>>()));
      suspension.OnFrameworkInitializationCompleted();

      // Load the saved view model state.
      var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();
      new MainWindow { DataContext = state }.Show();
      base.OnFrameworkInitializationCompleted();
    }
  }
}
