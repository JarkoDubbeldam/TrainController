using System.Collections.Concurrent;
using System.Reactive.Disposables;
using TrainAPI.Models;
using Z21;
using Z21.API;

namespace TrainAPI.Trackers;

public interface ISectionLocker {
  Task<IDisposable> AcquireSectionLock(int id);

}

public class SectionLocker : ISectionLocker {
  private readonly ConcurrentDictionary<int, SemaphoreSlim> locks = new();
  private readonly ITrackProvider trackProvider;
  private readonly IOccupancyTracker occupancyTracker;
  private readonly IZ21Client z21Client;
  private readonly ILogger<SectionLocker> logger;

  public SectionLocker(ITrackProvider trackProvider, IOccupancyTracker occupancyTracker, IZ21Client z21Client, ILogger<SectionLocker> logger) {
    this.trackProvider = trackProvider;
    this.occupancyTracker = occupancyTracker;
    this.z21Client = z21Client;
    this.logger = logger;
  }

  public async Task<IDisposable> AcquireSectionLock(int id) {
    logger.LogDebug("Received request for section {id}", id);
    var track = trackProvider.GetById(id);

    // Check and wait for the section to be empty:
    while (occupancyTracker.IsOccupied(track.SectionId)) {
      logger.LogDebug("Section {id} is occupied.", id);
      await Task.Delay(TimeSpan.FromMilliseconds(500));
    }

    logger.LogDebug("Acquiring lock for section {id}", id);
    var semaphore = locks.GetOrAdd(track.SectionId, new SemaphoreSlim(1, 1));
    await semaphore.WaitAsync();
    logger.LogDebug("Acquired lock for section {id}", id);

    if (occupancyTracker.IsOccupied(track.SectionId)) {
      throw new Exception("Huh, this shouldn't be occupied yet if there's a fresh lock on this section.");
    }

    await ActivateTrackSection(track);

    return Disposable.Create(() => {
      logger.LogDebug("Released lock for section {id}", id);
      semaphore.Release();
    });
  }

  private async Task ActivateTrackSection(TrackSection track) {
    foreach(var turnoutConfiguration in track.TurnoutConfigurations) {
      logger.LogDebug("Setting turnout {id} to position {position}", turnoutConfiguration.TurnoutId, turnoutConfiguration.TurnoutPosition);
      z21Client.SetTurnout(new SetTurnoutRequest {
        Address = (short)turnoutConfiguration.TurnoutId,
        TurnoutPosition = turnoutConfiguration.TurnoutPosition
      });
      await Task.Delay(TimeSpan.FromMilliseconds(500));
    }
  }
}
