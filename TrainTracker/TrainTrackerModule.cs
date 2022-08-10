using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace TrainTracker;
public class TrainTrackerModule : Module {
  protected override void Load(ContainerBuilder builder) {
    builder.RegisterType<TrainTracker>();
    base.Load(builder);
  }
}
