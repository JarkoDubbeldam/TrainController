using Microsoft.Extensions.Logging;
using Track;
using TrainTracker;
using Z21.Domain;

namespace TrainController;
public sealed class TrainStopper : IDisposable {
  private readonly TrainTracker.TrainTracker trainTracker;
  private readonly ILogger<TrainStopper> logger;
  private readonly CancellationTokenSource cts;
  private readonly TimeSpan checkInterval = TimeSpan.FromMilliseconds(500);

  public TrainStopper(TrainTracker.TrainTracker trainTracker, ILogger<TrainStopper> logger) {
    this.trainTracker = trainTracker;
    this.logger = logger;
    cts = new CancellationTokenSource();
  }

  public void Dispose() {
    cts.Cancel();
    ((IDisposable)cts).Dispose();
  }

  private async Task Run(CancellationToken cancellationToken) {
    while (true) {
      cancellationToken.ThrowIfCancellationRequested();
      CheckAll();
      await Task.Delay(checkInterval, cancellationToken);
    }
  }

  private void CheckAll() {
    foreach(var trainLocation in trainTracker.TrainLocations) {
      if (DetermineIfStopNeeded(trainLocation)) {
        logger.LogInformation("Stopping train {train}.", trainLocation.Train.Name);
        trainLocation.Train.SetSpeed(trainLocation.Train.Speed.WithSpeed(Speed.Stop));
      }
    }
  }

  private bool DetermineIfStopNeeded(TrainLocation trainLocation) {
    if (trainLocation.Train.Speed.Speed <= 0) {
      logger.LogInformation("Train {train} is standing still.", trainLocation.Train.Name);
      return false;
    } else {
      // Check if any sections contain a red signal.
      var redSignals = trainLocation
        .OccupiedConnections
        .Select(connection => connection.Signal)
        .Where(signal => signal != null)
        .Where(signal => signal!.SignalState == SignalColour.Red)
        .ToList();
      if (redSignals.Any()) {
        logger.LogInformation(
          "Train {train} is in front of a red signal. {signalId}",
          trainLocation.Train.Name, redSignals.First()!.Id
        );
        return true;
      }  

      // Check if all possible next segments are active and unoccupied
      foreach(var occupiedSection in trainLocation.OccupiedConnections) {
        var nextSection = occupiedSection.WalkActiveSections().First();
        // Check if next section is unavailable, either due to end of the track, or due to inactive turnout state.
        if(nextSection.TrackConnectionState != TrackConnection.TrackConnectionIterator.TrackConnectionStateEnum.Active) {
          logger.LogInformation(
            "No active section for train {train} after {section}.",
            trainLocation.Train.Name,
            occupiedSection.ViaSection.SectionId
          );
          return true;
        }
        // Check if next section is occupied by another train than current.
        var isOccupied = nextSection.TrackConnection.ViaSection.IsOccupied;
        var isNotAlreadyOccupiedByThisTrain = !trainLocation.OccupiedConnections
          .Contains(nextSection.TrackConnection);
        var isOccupiedByAnotherTrain = isOccupied && isNotAlreadyOccupiedByThisTrain;
        if(isOccupiedByAnotherTrain) {
          logger.LogInformation(
            "Next section {section} for train {train} is occupied.",
            nextSection.TrackConnection.ViaSection.SectionId,
            trainLocation.Train.Name
          );
          return true;
        }
      }
    }
    return false;
  }
}
