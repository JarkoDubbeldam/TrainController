using System;
using System.Net;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Z21;
using Z21.Domain;

var services = new ServiceCollection();
services.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Trace));
services.AddZ21(s => s.Z21Endpoint = new System.Net.IPEndPoint(IPAddress.Parse("192.168.0.111"), 21105));

using var collection = services.BuildServiceProvider();

var z21Client = collection.GetService<IZ21Client>();

using var _ = z21Client.LocomotiveInformationChanged.Do(PrintLoco).Subscribe();

using var __ = z21Client.OccupancyStatusChanged.Do(PrintLoco).Subscribe();

using var ___ = z21Client.TurnoutInformationChanged.Do(PrintLoco).Subscribe();

void PrintLoco<T>(T information) {
  Console.WriteLine(information);
}

Console.ReadLine();
