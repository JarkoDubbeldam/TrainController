using Microsoft.Extensions.Hosting;
using TrainAPI.Data;
using TrainAPI.Models;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainAPI.Trackers;

public interface ITrainTracker : ITracker<TrainWithInformation> { }

public class TrainTracker : IHostedService, ITrainTracker {
  private readonly IServiceProvider serviceProvider;
  private readonly IZ21Client z21Client;
  private readonly Dictionary<int, TrainWithInformation> trainData = new();
  private readonly HashSet<int> existingTrains = new();
  private IDisposable? listener;

  public TrainTracker(IServiceProvider serviceProvider, IZ21Client z21Client) {
    this.serviceProvider = serviceProvider;
    this.z21Client = z21Client;
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    using (var scope = serviceProvider.CreateScope()) {
      var trainAPIContext = scope.ServiceProvider.GetRequiredService<TrainAPIContext>();
      await foreach (var train in trainAPIContext.Train.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
        var trainInformation = await z21Client.GetLocomotiveInformation(new LocomotiveInformationRequest {
          LocomotiveAddress = (short)train.Id
        });
        existingTrains.Add(train.Id);
        trainData[train.Id] = new TrainWithInformation {
          Id = train.Id, 
          Name = train.Name,
          TrainFunctions = trainInformation.TrainFunctions,
          Speed = trainInformation.TrainSpeed
        };
      }

    }
    listener = z21Client.LocomotiveInformationChanged.Subscribe(UpdateTrain);
  }

  private void UpdateTrain(LocomotiveInformation newInformation) {
    if(trainData.TryGetValue(newInformation.Address, out var existingValue)) {
      existingValue.TrainFunctions = newInformation.TrainFunctions;
      existingValue.Speed = newInformation.TrainSpeed;
    } else {
      var newValue = new TrainWithInformation {
        Id = newInformation.Address,
        Name = "",
        Speed = newInformation.TrainSpeed,
        TrainFunctions = newInformation.TrainFunctions
      };
      trainData[newInformation.Address] = newValue;
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken) {
    listener?.Dispose();
    using var scope = serviceProvider.CreateScope();
    var trainAPIContext = scope.ServiceProvider.GetRequiredService<TrainAPIContext>();

    var existingTrains = trainData.Values.IntersectBy(this.existingTrains, t => t.Id);
    trainAPIContext.Train.UpdateRange(existingTrains);
    var newTrains = trainData.Values.ExceptBy(this.existingTrains, t => t.Id);
    trainAPIContext.Train.AddRange(newTrains);
    await trainAPIContext.SaveChangesAsync(cancellationToken);
  }

  public TrainWithInformation Get(int id) => trainData[id];
  public void Add(int id, TrainWithInformation value) => trainData[id] = value;
  public bool Remove(int id) => trainData.Remove(id);
  public ICollection<TrainWithInformation> List() => trainData.Values;
}
