using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Autofac;
using Z21;

namespace TrainRepository {
  public class TrainRepositoryModule : Module {
    private readonly IPEndPoint iPEndPoint;

    public TrainRepositoryModule(IPEndPoint iPEndPoint) {
      this.iPEndPoint = iPEndPoint;
    }

    protected override void Load(ContainerBuilder builder) {
      builder.RegisterInstance(iPEndPoint);
      builder.Register(c => new System.Net.Sockets.UdpClient(12345));
      builder.RegisterType<Z21.UdpClient>().As<IUdpClient>();
      builder.RegisterType<Z21Client>().As<IZ21Client>().InstancePerLifetimeScope();

      builder.RegisterType<TrainRepository>().As<IRepository<Train>>();
      builder.RegisterType<TurnoutRepository>().As<IRepository<Turnout>>();

      builder.RegisterType<DigiKeijsInteractionHandler>().As<ITurnoutInteractionHandler>();
      base.Load(builder);
    }
  }
}
