using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace TrainController;
public class TrainControllerModule : Module {
  protected override void Load(ContainerBuilder builder) {
    builder.RegisterType<TrainStopper>();
    base.Load(builder);
  }
}
