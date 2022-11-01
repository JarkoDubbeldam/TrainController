using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TrainAPI.Data;
using Z21.Domain;

namespace TrainAPI.Trackers;

public sealed class TrainPositionTracker : IHostedService, IDisposable, ITrainPositionTracker {
  private const string KEY = "TrainLocations";
  private readonly IOccupancyTracker occupancyTracker;
  private readonly ITrainTracker trainTracker;
  private readonly ITrackProvider trackProvider;
  private readonly IServiceProvider serviceProvider;
  private readonly ILogger<TrainPositionTracker> logger;
  private readonly CancellationTokenSource cts = new();
  private List<TrainPosition> trainPositions = new();

  public TrainPositionTracker(
    IOccupancyTracker occupancyTracker,
    ITrainTracker trainTracker,
    ITrackProvider trackProvider,
    IServiceProvider serviceProvider,
    ILogger<TrainPositionTracker> logger) {
    this.occupancyTracker = occupancyTracker;
    this.trainTracker = trainTracker;
    this.trackProvider = trackProvider;
    this.serviceProvider = serviceProvider;
    this.logger = logger;
  }

  public void Dispose() {
    ((IDisposable)cts).Dispose();
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    await trackProvider.Load();
    using (var scope = serviceProvider.CreateScope())
    using (var db = scope.ServiceProvider.GetRequiredService<TrainAPIContext>()) {
      var data = await db.KeyValue.SingleAsync(x => x.Id == KEY, cancellationToken: cancellationToken);
      trainPositions = JsonConvert.DeserializeObject<List<TrainPosition>>(data.Value) ?? throw new ArgumentNullException();
    }

    _ = Task.Run(() => ExecuteAsync(cts.Token));
  }

  public async Task StopAsync(CancellationToken cancellationToken) {
    cts.Cancel();
    Dispose();
    using (var scope = serviceProvider.CreateScope())
    using (var db = scope.ServiceProvider.GetRequiredService<TrainAPIContext>()) {
      var data = await db.KeyValue.SingleAsync(x => x.Id == KEY, cancellationToken: cancellationToken);
      data.Value = JsonConvert.SerializeObject(trainPositions);
      await db.SaveChangesAsync(cancellationToken);
    }
  }

  public TrainPosition GetTrainPosition(int trainId) => trainPositions.Single(x => x.TrainId == trainId);

  private async Task ExecuteAsync(CancellationToken stoppingToken) {
    while (!stoppingToken.IsCancellationRequested) {
      var delay = Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
      try {
        RunUpdate();
      } catch(Exception e) {
        logger.LogError(e, "!!");
      }
      await delay;
    }
  }


  private void RunUpdate() {
    // Check that all occupied sections are still occupied
    var occupiedSections = occupancyTracker.OccupiedSections().ToHashSet();
    // Handle turnarounds
    foreach (var trainPosition in trainPositions) {
      var train = trainTracker.Get(trainPosition.TrainId);
      var turnedAround = train.Speed.DrivingDirection != trainPosition.LastSeenDirection;
      if (turnedAround) {
        logger.LogInformation("Train {id} turned around.", trainPosition.TrainId);
        trainPosition.LastSeenDirection = train.Speed.DrivingDirection;
        trainPosition.OccupiedTrackSections = new LinkedList<int>(trainPosition.OccupiedTrackSections
          .Reverse()
          .Select(x => trackProvider.GetOppositeDirection(x))
          .Where(x => x != null)
          .Select(x => x!.Id));
      }
    }

    foreach (var trainPosition in trainPositions) {
      var train = trainTracker.Get(trainPosition.TrainId);
      if(train.Speed.Speed == Speed.Stop || train.Speed.Speed == Speed.EmergencyStop) {
        continue;
      }

      var newPositions = new LinkedList<int>(trainPosition.OccupiedTrackSections);
      // Add new section to head.
      var nextSection = trackProvider.GetNextActiveSection(newPositions.Last.Value);
      if (nextSection != null && !trainPosition.OccupiedTrackSections.Contains(nextSection.Id) && occupiedSections.Contains(nextSection.SectionId)) {
        trainPosition.TrainLost = false;
        newPositions.AddLast(nextSection.Id);
        logger.LogInformation("Train {id} entered section {sectionId}", trainPosition.TrainId, nextSection.Id);
      }

      // If last known position is empty, remove it.
      var section = trackProvider.GetById(newPositions.First.Value);
      if (!occupiedSections.Contains(section.SectionId)) {
        if (newPositions.Count == 1) {
          if (!trainPosition.TrainLost) {
            logger.LogWarning("Lost track of train {id}", trainPosition.TrainId);
          }
          trainPosition.TrainLost = true;
        } else {
          logger.LogInformation("Train {id} left section {sectionId}", trainPosition.TrainId, newPositions.First.Value);
          newPositions.RemoveFirst();
        }
      }

      trainPosition.OccupiedTrackSections = newPositions;
    }    
  }
}

public class TrainPosition {
  public int TrainId { get; set; }
  public DrivingDirection LastSeenDirection { get; set; }
  public LinkedList<int> OccupiedTrackSections { get; set; }
  public bool TrainLost { get; set; }
}
