using Microsoft.Extensions.Logging;
using Track;
using TrainTracker;
using Z21.Domain;

namespace TrainController;
public sealed class TrainStopper : IDisposable {
  private readonly Task<TrainTracker.TrainTracker> trainTracker;
  private readonly ILogger<TrainStopper> logger;
  private readonly CancellationTokenSource cts;
  private readonly TimeSpan checkInterval = TimeSpan.FromMilliseconds(100);

  public TrainStopper(Task<TrainTracker.TrainTracker> trainTracker, ILogger<TrainStopper> logger) {
    this.trainTracker = trainTracker;
    this.logger = logger;
    cts = new CancellationTokenSource();
    Task.Run(() => Run(cts.Token));
  }

  public void Dispose() {
    cts.Cancel();
    ((IDisposable)cts).Dispose();
  }

  private async Task Run(CancellationToken cancellationToken) {
    var tracker = await trainTracker;
    while (true) {
      cancellationToken.ThrowIfCancellationRequested();
      CheckAll(tracker);
      await Task.Delay(checkInterval, cancellationToken);
    }
  }

  private void CheckAll(TrainTracker.TrainTracker trainTracker) {
    foreach(var trainLocation in trainTracker.TrainLocations) {
      try {
        if (DetermineIfStopNeeded(trainLocation)) {
          logger.LogInformation("Stopping train {train}.", trainLocation.Train.Name);
          trainLocation.Train.SetSpeed(trainLocation.Train.Speed.WithSpeed(Speed.Stop));
        }
      } catch(Exception e){
        logger.LogError(e, "oops");
      }
    }
  }

  private bool DetermineIfStopNeeded(TrainLocation trainLocation) {
    if (trainLocation.Train.Speed.Speed <= 0) {
      logger.LogDebug("Train {train} is standing still.", trainLocation.Train.Name);
      return false;
    } else {
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
        var hasRedSignal = occupiedSection.Signal?.SignalState == SignalColour.Red;
        var isNotAlreadyOccupiedByThisTrain = !trainLocation.OccupiedConnections
          .Select(o => o.ViaSection.SectionId)
          .Contains(nextSection.TrackConnection.ViaSection.SectionId);
        var isOccupiedByAnotherTrain = isOccupied && isNotAlreadyOccupiedByThisTrain;
        var hasRedSignalNotDueToThisTrain = hasRedSignal && isNotAlreadyOccupiedByThisTrain;
        if(isOccupiedByAnotherTrain) {
          logger.LogInformation(
            "Next section {section} for train {train} is occupied.",
            nextSection.TrackConnection.ViaSection.SectionId,
            trainLocation.Train.Name
          );
          return true;
        } else if (hasRedSignalNotDueToThisTrain) {
          logger.LogInformation(
            "Train {train} is in front of a red signal. {signalId}",
            trainLocation.Train.Name, occupiedSection.Signal!.Id
          );
          return true;
        }
      }
    }
    return false;
  }
}
