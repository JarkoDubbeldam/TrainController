using Microsoft.Extensions.Hosting;
using TrainAPI.Data;
using TrainAPI.Models;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainAPI.Trackers;

public interface ITurnoutTracker : ITracker<TurnoutPosition> { }

public class TurnoutTracker : IHostedService, ITurnoutTracker {
  private readonly IServiceProvider serviceProvider;
  private readonly IZ21Client z21Client;
  private readonly Dictionary<int, TurnoutPosition> turnoutPositions = new();
  private readonly HashSet<int> existingTurnouts = new();
  private IDisposable? listener;

  public TurnoutTracker(IServiceProvider serviceProvider, IZ21Client z21Client) {
    this.serviceProvider = serviceProvider;
    this.z21Client = z21Client;
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    using (var scope = serviceProvider.CreateScope()) {
      var trainAPIContext = scope.ServiceProvider.GetRequiredService<TrainAPIContext>();
      await foreach (var turnout in trainAPIContext.Turnout.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
        var position = await z21Client.GetTurnoutInformation(new TurnoutInformationRequest {
          Address = (short)turnout.Id
        });
        existingTurnouts.Add(turnout.Id);
        turnoutPositions[turnout.Id] = position.TurnoutPosition;
      }

    }
    listener = z21Client.TurnoutInformationChanged.Subscribe(HandleTurnoutUpdate);
  }

  private void HandleTurnoutUpdate(TurnoutInformation newInformation) => turnoutPositions[newInformation.Address] = newInformation.TurnoutPosition;

  public async Task StopAsync(CancellationToken cancellationToken) {
    listener?.Dispose();
    using var scope = serviceProvider.CreateScope();
    var trainAPIContext = scope.ServiceProvider.GetRequiredService<TrainAPIContext>();

    trainAPIContext.Turnout.AddRange(turnoutPositions.Keys.Except(existingTurnouts).Select(x => new Turnout { Id = x }));
    await trainAPIContext.SaveChangesAsync(cancellationToken);
  }

  public TurnoutPosition Get(int id) => turnoutPositions[id];
  public void Add(int id, TurnoutPosition value) => turnoutPositions[id] = value;
  public bool Remove(int id) => throw new NotImplementedException();
  public ICollection<TurnoutPosition> List() => turnoutPositions.Values;
}
