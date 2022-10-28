using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainAPI.Trackers;

public interface IOccupancyTracker {
  bool IsOccupied(int id);
  IEnumerable<int> OccupiedSections();
}

public class OccupancyTracker : IHostedService, IOccupancyTracker {
  private readonly IZ21Client z21Client;
  private OccupancyStatus occupancy;
  private IDisposable? listener;

  public OccupancyTracker(IZ21Client z21Client) {
    this.z21Client = z21Client;
  }
  public bool IsOccupied(int id) => occupancy.Occupancies[id];
  public IEnumerable<int> OccupiedSections() => occupancy.Occupancies.OfType<bool>().Select((x, i) => (x, i)).Where(x => x.x).Select(x => x.i);
  public async Task StartAsync(CancellationToken cancellationToken) {
    occupancy = await z21Client.GetOccupancyStatus(new OccupancyStatusRequest { GroupIndex = 0 });

    listener = z21Client.OccupancyStatusChanged.Subscribe(UpdateOccupancy);    
  }

  private void UpdateOccupancy(OccupancyStatus obj) => occupancy = obj;

  public Task StopAsync(CancellationToken cancellationToken) {
    listener?.Dispose();
    return Task.CompletedTask;
  }
}
