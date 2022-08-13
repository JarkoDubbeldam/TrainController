using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace TrainTracker;
public class TrainTrackerModule : Module {
  protected override void Load(ContainerBuilder builder) {
    builder.RegisterType<TrainTrackerFactory>().SingleInstance();
    builder.Register<Func<string, Task<TrainTracker>>>(c => {
      return s => c.Resolve<TrainTrackerFactory>().Build(s);
    });
    builder.Register(c => {
      var factory = c.Resolve<TrainTrackerFactory>();
      return factory.GetAny();
    });
    base.Load(builder);
  }
}
