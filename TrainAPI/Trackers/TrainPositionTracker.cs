﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TrainAPI.Data;
using Z21.Domain;

namespace TrainAPI.Trackers;

public sealed class TrainPositionTracker : IHostedService, IDisposable {
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
      var data = await db.KeyValue.SingleAsync(x => x.Id == KEY);
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

  private async Task ExecuteAsync(CancellationToken stoppingToken) {
    while (!stoppingToken.IsCancellationRequested) {
      var delay = Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
      RunUpdate();
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
        trainPosition.OccupiedTrackSections = trainPosition.OccupiedTrackSections
          .Select(x => trackProvider.GetOppositeDirection(x))
          .Where(x => x != null)
          .Select(x => x!.Id)
          .ToList();
      }
    }

    foreach (var trainPosition in trainPositions) {
      var newPositions = new List<int>(trainPosition.OccupiedTrackSections);
      foreach (var occupiedSection in trainPosition.OccupiedTrackSections) {
        var section = trackProvider.GetById(occupiedSection);
        // Remove unoccupied sections from the list
        if (!occupiedSections.Contains(section.SectionId)) {
          newPositions.Remove(occupiedSection);
          //occupiedSections.Remove(section.SectionId);
          logger.LogInformation("Train {id} left section {sectionId}", trainPosition.TrainId, section.Id);
        }
        // Try to account for new occupations that are not already known.
        var nextSection = trackProvider.GetNextActiveSection(section.Id);
        if (nextSection != null && !trainPosition.OccupiedTrackSections.Contains(nextSection.Id) && occupiedSections.Contains(nextSection.SectionId)) {
          newPositions.Add(nextSection.Id);
          //occupiedSections.Remove(nextSection.SectionId);
          logger.LogInformation("Train {id} entered section {sectionId}", trainPosition.TrainId, nextSection.Id);
        }
      }
      trainPosition.OccupiedTrackSections = newPositions;
    }

    //if (occupiedSections.Count > 0) {
    //  // Handle unaccounted for sections.
    //  logger.LogInformation("The following sections are unaccounted for: {sections}", string.Join(",", occupiedSections.Select(x => x.ToString())));
    //}
  }
}

public class TrainPosition {
  public int TrainId { get; set; }
  public DrivingDirection LastSeenDirection { get; set; }
  public List<int> OccupiedTrackSections { get; set; }
}