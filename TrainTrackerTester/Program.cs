using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Autofac;
using Track;
using TrainRepository;
using TrainTracker;
using Z21;

namespace TrainTrackerTester {
  internal class Program {
    static async Task Main(string[] args) {
      var containerBuilder = new ContainerBuilder();
      var ipAdress = new IPAddress(new byte[] { 192, 168, 0, 111 });
      var endpoint = new IPEndPoint(ipAdress, 21105);
      var startingSection = 4;

      containerBuilder.RegisterModule(new TrainRepositoryModule(endpoint));
      containerBuilder.RegisterModule<TrainTrackerModule>();
      containerBuilder.RegisterModule<TrackModule>();
      using var disposable = new CompositeDisposable();
      var container = containerBuilder.Build();
      container.DisposeWith(disposable);

      var json = await File.ReadAllTextAsync("layoutv4.json");
      var trackRepository = container.Resolve<Func<string, TrackRepository>>()(json);
      var trackerRepository = await container.Resolve<TrainTrackerFactory>().Build("occupancies.json");
      
      await Task.Delay(10000);

      Console.WriteLine("Done setting up. Press any key to exit.");
      Console.ReadLine();
    }
  }
}
