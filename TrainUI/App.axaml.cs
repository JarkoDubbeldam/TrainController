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
using TrainController;
using TrainRepository;
using TrainTracker;
using TrainUI.Converters;
using TrainUI.ViewModels;
using TrainUI.Views;

namespace TrainUI {
  public class App : Application {

    public override void Initialize() {
      AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
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
      Locator.Current.GetService<TrainTrackerFactory>().Build(state.OccupancyFileName);
      Locator.Current.GetService<TrainStopper>();
      new MainWindow { DataContext = state }.Show();
      base.OnFrameworkInitializationCompleted();
    }
  }
}
