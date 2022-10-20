using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace Track;
public class TrackModule : Module {
  protected override void Load(ContainerBuilder builder) {
    builder.RegisterType<TrackRepositoryFactory>().SingleInstance();
    builder.Register<Func<string, TrackRepository>>(c => {
      var factory = c.Resolve<TrackRepositoryFactory>();
      return s => factory.Build(s);
    });
    builder.Register(c => {
      var factory = c.Resolve<TrackRepositoryFactory>();
      return factory.GetAny();
    });
    base.Load(builder);
  }
}
