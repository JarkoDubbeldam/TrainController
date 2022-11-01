using System.Collections.Concurrent;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainAPI.Trackers;

public interface INavigator {
  Task Navigate(int trainId, DrivingDirection drivingDirection, List<int> trackSectionIds);
}

public class Navigator : INavigator {
  private readonly IZ21Client z21Client;
  private readonly ISectionLocker sectionLocker;
  private readonly ITrainPositionTracker trainPositionTracker;
  private readonly ILogger<Navigator> logger;

  public Navigator(IZ21Client z21Client, ISectionLocker sectionLocker, ITrainPositionTracker trainPositionTracker, ILogger<Navigator> logger) {
    this.z21Client = z21Client;
    this.sectionLocker = sectionLocker;
    this.trainPositionTracker = trainPositionTracker;
    this.logger = logger;
  }

  public async Task Navigate(int trainId, DrivingDirection drivingDirection, List<int> trackSectionIds) {
    var position = trainPositionTracker.GetTrainPosition(trainId);

    // Remove tracksections the train already sits on.
    trackSectionIds = trackSectionIds.SkipWhile(x => position.OccupiedTrackSections.Contains(x)).ToList();

    var queue = new ConcurrentQueue<NavigatingSection>();
    var lockingTask = Task.Run(async () => {
      foreach (var trackSectionId in trackSectionIds) {
        var claim = await sectionLocker.AcquireSectionLock(trackSectionId);
        queue.Enqueue(new NavigatingSection {
          SectionClaim = claim,
          TrackSectionId = trackSectionId
        });
        await Task.Delay(TimeSpan.FromMilliseconds(500));
      }
    });
    var runningTask = RunLocomotive(trainId, drivingDirection, trackSectionIds[^1], queue);
    await Task.WhenAll(lockingTask, runningTask);
  }

  private async Task RunLocomotive(int trainId, DrivingDirection drivingDirection, int goalId, ConcurrentQueue<NavigatingSection> navigatingSections) {
    var stopped = true;
    void Stop() {
      z21Client.SetTrainSpeed(new TrainSpeedRequest {
        TrainAddress = (short)trainId,
        TrainSpeed = new TrainSpeed(SpeedStepSetting.Step128, drivingDirection, Speed.Stop)
      });
      stopped = true;
    }
    void Go() {
      z21Client.SetTrainSpeed(new TrainSpeedRequest {
        TrainAddress = (short)trainId,
        TrainSpeed = new TrainSpeed(SpeedStepSetting.Step128, drivingDirection, (Speed)80)
      });
      stopped = false;
    }
    logger.LogDebug("Starting navigating for Train {id}", trainId);
    Stop();
    var occupiedSections = new LinkedList<NavigatingSection>();
    var trainPosition = trainPositionTracker.GetTrainPosition(trainId);
    while (!occupiedSections.Any(x => x.TrackSectionId == goalId)) {
      if (trainPosition.TrainLost) {
        if (!stopped) {
          logger.LogWarning("Train {id} was lost. Stopping", trainId);
          Stop();
        }
      } else if (!navigatingSections.TryPeek(out var nextSection)) {
        // There's no next section available yet. So we stop.
        if (!stopped) {
          logger.LogDebug("Train {id} is waiting for the next section to become available.", trainId);
          Stop();
        }
      } else {
        // There's a next section. Start going if we weren't already.
        if (stopped) {
          logger.LogDebug("Is moving.", trainId);
          Go();
        }

        // Check if we've already reached this section
        if (trainPosition.OccupiedTrackSections.Contains(nextSection.TrackSectionId)) {
          // if so, add it to occupiedSections instead.
          if (navigatingSections.TryDequeue(out nextSection)) {
            logger.LogDebug("Train {id} has reached section {sectionId}", trainId, nextSection.TrackSectionId);
            occupiedSections.AddLast(nextSection);
          } else {
            throw new Exception("huh?");
          }
        }

        // Check if we have left our old sections:
        var firstSection = occupiedSections.First?.Value;
        if (firstSection != null && !trainPosition.OccupiedTrackSections.Contains(firstSection.TrackSectionId)) {
          occupiedSections.RemoveFirst();
          firstSection.SectionClaim.Dispose();
        }       
      }

      await Task.Delay(TimeSpan.FromMilliseconds(100));
    }

    //Destination reached.
    Stop();

    foreach(var section in occupiedSections) {
      section.SectionClaim.Dispose();
    }
  }

  private class NavigatingSection {
    public int TrackSectionId { get; set; }
    public IDisposable SectionClaim { get; set; }
  }
}
