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

using TrainRepository;

using TrainUI.Converters;
using TrainUI.Models;
using TrainUI.ViewModels;
using TrainUI.Views;

namespace TrainUI {
  public class App : Application {
    private IContainer container;

    public override void Initialize() {
      AvaloniaXamlLoader.Load(this);

      var builder = new ContainerBuilder();
      builder.RegisterModule<TrainUIModule>();
      container = builder.Build();
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
      var factory = container.Resolve<Func<MainWindowModel, MainWindowViewModel>>();
      RxApp.SuspensionHost.CreateNewAppState = () => factory(new MainWindowModel { 
        Trains = new[] { 
          new TrainModel { 
            Name = "Db loc", 
            Address = 3, 
            TrainFunctions = new[] { 
              new TrainFunctionModel {
                Name = "Lights",
                Mask = Z21.Domain.TrainFunctions.Lights
              } 
            }
          }
        }
      });
      RxApp.SuspensionHost.SetupDefaultSuspendResume(new JsonSuspensionDriver(new JsonSuspensionSettings { Filename = "appstate.json" }, container.Resolve<IList<JsonConverter>>()));
      suspension.OnFrameworkInitializationCompleted();

      // Load the saved view model state.
      var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();
      new MainWindow { DataContext = state }.Show();
      base.OnFrameworkInitializationCompleted();
    }
  }
}
