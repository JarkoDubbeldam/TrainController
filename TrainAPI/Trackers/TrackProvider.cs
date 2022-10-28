using Microsoft.EntityFrameworkCore;
using TrainAPI.Data;
using TrainAPI.Models;

namespace TrainAPI.Trackers;

public class TrackProvider : ITrackProvider {
  private readonly IServiceProvider serviceProvider;
  private readonly ITurnoutTracker turnoutTracker;
  private List<TrackSection>? tracks;
  private Dictionary<int, TrackSection> trackById;
  private ILookup<int, TrackSection> tracksBySectionId;

  public TrackProvider(IServiceProvider serviceProvider, ITurnoutTracker turnoutTracker) {
    this.serviceProvider = serviceProvider;
    this.turnoutTracker = turnoutTracker;
  }

  public async Task Load() {
    if (tracks == null) {
      using var scope = serviceProvider.CreateScope();
      using var trainAPIContext = scope.ServiceProvider.GetRequiredService<TrainAPIContext>();
      tracks = await trainAPIContext.TrackSection
        .Include(x => x.ToBoundary)
        .Include(x => x.FromBoundary)
        .Include(x => x.TurnoutConfigurations)
        .ToListAsync();
      trackById = tracks.ToDictionary(x => x.Id);
      tracksBySectionId = tracks.ToLookup(x => x.SectionId);
    }
  }

  public TrackSection GetById(int id) => trackById[id];
  public ICollection<TrackSection> GetBySectionId(int sectionId) => tracksBySectionId[sectionId].ToList();
  public TrackSection? GetOppositeDirection(int id) {
    var initialTrack = trackById[id];
    return initialTrack
      .ToBoundary
      .FromTrackSections
      .Where(x => x.SectionId == initialTrack.SectionId)
      .Where(x => x.TurnoutConfigurations
        .Join(
          initialTrack.TurnoutConfigurations,
          left => left.TurnoutId,
          right => right.TurnoutId,
          (left, right) => left.TurnoutPosition == right.TurnoutPosition
        )
        .All(x => x))
      .SingleOrDefault();
  }

  public TrackSection? GetNextActiveSection(int fromId) {
    var initialTrack = trackById[fromId];
    return initialTrack
      .ToBoundary
      .FromTrackSections
      .Where(x => x.SectionId != initialTrack.SectionId)
      .Where(x => x.TurnoutConfigurations.All(x => x.TurnoutPosition == turnoutTracker.Get(x.TurnoutId)))
      .SingleOrDefault();
  }
}
