using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Text.Json;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TrainController.Turnouts;

using Trains.DataAccess.Services;

namespace TrainController.Segments;
internal class SegmentController : BackgroundService, IController<Segment> {
  private readonly Subject<Segment> subject = new();
  private readonly ConcurrentDictionary<int, Segment> segments = new();
  private readonly IController<Turnout> turnoutController;
  private readonly ISegmentService segmentService;
  private readonly ILogger<SegmentController> logger;
  private bool hasFetchedYet = false;

  public SegmentController(IController<Turnout> turnoutController, ISegmentService segmentService, ILogger<SegmentController> logger) {
    this.turnoutController = turnoutController;
    this.segmentService = segmentService;
    this.logger = logger;
  }

  public IObservable<Segment> Observable => subject;

  public Task Apply(Segment value) {
    // Validation
    if (value.SegmentMode == SegmentMode.Enabling || value.SegmentMode == SegmentMode.Disabling) {
      throw new InvalidOperationException();
    }

    var result = segments.AddOrUpdate(value.Configuration.Id, value, (key, existingValue) => {
      if (existingValue.SegmentMode != value.SegmentMode) {
        return existingValue with {
          SegmentMode = value.SegmentMode,
          Configuration = value.Configuration
        };
      }
      return existingValue;
    });

    subject.OnNext(result);
    return Task.CompletedTask;
  }


  public async Task<Segment?> Get(int id) {
    if (!hasFetchedYet) {
      await FetchDatabase();
    }

    return segments.GetValueOrDefault(id);
  }

  public async Task<IReadOnlyDictionary<int, Segment>> List() {
    if (!hasFetchedYet) {
      await FetchDatabase();
    }

    return segments;
  }

  protected async override Task ExecuteAsync(CancellationToken stoppingToken) {
    while (!stoppingToken.IsCancellationRequested) {
      await RunUpdateLoop();
      await Task.Delay(500, stoppingToken);
    }
  }

  private async Task RunUpdateLoop() {
    foreach (var segment in segments.Values) {
      var mutableSegment = segment;

      var turnoutsAreGoodForEnabled = true;
      foreach (var turnoutConfiguration in mutableSegment.Configuration.Turnouts) {
        var turnoutStatus = await turnoutController.Get(turnoutConfiguration.TurnoutId);
        if (turnoutStatus is null) {
          turnoutsAreGoodForEnabled = false;
          continue;
        }

        if (turnoutStatus.TurnoutStatus != turnoutConfiguration.TurnoutMode) {
          turnoutsAreGoodForEnabled = false;
          if (mutableSegment.SegmentMode == SegmentMode.Enabled) {
            var newTurnout = turnoutStatus with { TurnoutMode = turnoutConfiguration.TurnoutMode };

            await turnoutController.Apply(newTurnout);
          }
        }
      }

      // If the segment is intended to be enabled...
      if (mutableSegment.SegmentMode == SegmentMode.Enabled && mutableSegment.ActualSegmentMode != SegmentMode.Enabled) {
        // And it is...
        if (turnoutsAreGoodForEnabled) {
          // Then mark as enabled
          mutableSegment = mutableSegment with { ActualSegmentMode = SegmentMode.Enabled };
        } else {
          // If it isn't, we should mark it as enabling.
          mutableSegment = mutableSegment with { ActualSegmentMode = SegmentMode.Enabling };
        }
      } else if (mutableSegment.SegmentMode == SegmentMode.Disabled && mutableSegment.ActualSegmentMode != SegmentMode.Disabled) {
        foreach (var turnout in mutableSegment.Configuration.Turnouts) {
          var turnoutStatus = await turnoutController.Get(turnout.TurnoutId);
          if (turnoutStatus is null) {
            continue;
          }

          var newTurnout = turnoutStatus with { TurnoutMode = TurnoutStatus.Unspecified };

          await turnoutController.Apply(newTurnout);
        }

        mutableSegment = mutableSegment with { ActualSegmentMode = SegmentMode.Disabled };
      }

      // If we changed something, save and update.
      if (mutableSegment != segment) {
        logger.LogInformation("Turnout {old} changed to {new}", segment, mutableSegment);
        segments.AddOrUpdate(mutableSegment.Configuration.Id, mutableSegment, (_, _) => mutableSegment);
        subject.OnNext(mutableSegment);
      }
    }
  }

  private async Task FetchDatabase() {
    var databaseSegments = await segmentService.ListSegments();

    foreach (var segment in databaseSegments) {
      segments.TryAdd(segment.Id, Deserialize(segment.Json));
    }

    hasFetchedYet = true;
  }

  private Segment Deserialize(string json) {
    try {
      var config = JsonSerializer.Deserialize<SegmentConfiguration>(json);
      if (config is null) {
        throw new InvalidDataException();
      }

      return new Segment(config);

    } catch (JsonException) {
      throw;
    }
  }
}
