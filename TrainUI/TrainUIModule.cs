using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Autofac;

using Newtonsoft.Json;

using TrainRepository;

using TrainUI.Converters;
using TrainUI.ViewModels;

namespace TrainUI {
  public class TrainUIModule : Module {
    protected override void Load(ContainerBuilder builder) {
      var ipAdress = new IPAddress(new byte[] { 192, 168, 0, 111 });
      var endpoint = new IPEndPoint(ipAdress, 21105);


      builder.RegisterModule(new TrainRepositoryModule(endpoint));
      builder.RegisterAssemblyTypes(ThisAssembly).Where(x => x.Namespace == typeof(MainWindowViewModel).Namespace);
      builder.RegisterAssemblyTypes(ThisAssembly).Where(x => x.IsAssignableTo<JsonConverter>()).As<JsonConverter>().AsSelf();
      builder.RegisterType<JsonSuspensionDriver>();

      base.Load(builder);
    }
  }
}
