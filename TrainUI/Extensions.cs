using System;
using System.Collections.Generic;
using System.Text;

using Autofac;

using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

using ReactiveUI;

using Splat;
using Splat.Autofac;

namespace TrainUI {
  internal static class Extensions {
    internal static void InitializeAvalonia(this IMutableDependencyResolver resolver) {
      resolver.RegisterConstant(new AvaloniaActivationForViewFetcher(), typeof(IActivationForViewFetcher));
      resolver.RegisterConstant(new AutoDataTemplateBindingHook(), typeof(IPropertyBindingHook));
      RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
    }

    internal static TAppBuilder UseAutofac<TAppBuilder, TModule>(this TAppBuilder builder)
      where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
      where TModule : Module, new() => builder.UseAutofac(new TModule());

    internal static TAppBuilder UseAutofac<TAppBuilder>(this TAppBuilder builder, Module autofacModule)
        where TAppBuilder : AppBuilderBase<TAppBuilder>, new() {
      return builder.AfterPlatformServicesSetup(_ => {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterModule(autofacModule);
        AutofacDependencyResolver resolver = new AutofacDependencyResolver(containerBuilder);
        Locator.SetLocator(resolver);
        Locator.CurrentMutable.InitializeSplat();
        Locator.CurrentMutable.InitializeReactiveUI();
        Locator.CurrentMutable.InitializeAvalonia();

        var container = containerBuilder.Build();
        resolver.SetLifetimeScope(container);
      });
    }
  }
}
