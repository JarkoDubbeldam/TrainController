using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Autofac;
using Track;
using TrainRepository;
using Z21;

namespace TrainTrackerTester {
  internal class Program {
    static async Task Main(string[] args) {
      var containerBuilder = new ContainerBuilder();
      var ipAdress = new IPAddress(new byte[] { 192, 168, 0, 111 });
      var endpoint = new IPEndPoint(ipAdress, 21105);
      var startingSection = 5;

      containerBuilder.RegisterModule(new TrainRepositoryModule(endpoint));
      var container = containerBuilder.Build();

      var json = await File.ReadAllTextAsync("layoutv4.json");
      var trackRepository = TrackRepository.FromJson(json);
      var client = container.Resolve<IZ21Client>();
      using (var disposable = new CompositeDisposable()) {
        trackRepository.SetupSubscriptions(client).DisposeWith(disposable);
        var trainRepository = container.Resolve<IRepository<Train>>();
        var train = await trainRepository.RegisterObject(5, "NS Traxx");
        var currentSection = trackRepository.Boundaries
          .SelectMany(x => x.Connections)
          .Distinct()
          .Where(x => x.ViaSection.SectionId == startingSection);
        var tracker = new TrainTracker.TrainTracker();
        tracker.AddTrain(train, currentSection);
        tracker.Setup(trackRepository);
        tracker.DisposeWith(disposable);

        Console.WriteLine("Done setting up. Press any key to exit.");
        Console.ReadLine();
      }
    }
  }
}
