using System;
using System.Collections.Generic;
using System.Linq;

namespace Track {
  public class TrackConnection {
    public TrackSection ViaSection { get; set; }
    public TrackSectionBoundary ToBoundary { get; set; }
    public SignalConfiguration? Signal { get; set; }

    internal List<TrackConnection> GetNextSections() =>
      ToBoundary.Connections.Where(x => x.ViaSection.SectionId != ViaSection.SectionId).ToList();
    
    public IEnumerable<TrackConnectionIterator> WalkActiveSections() {
      var current = new TrackConnectionIterator(this);
      while(current.TrackConnectionState == TrackConnectionIterator.TrackConnectionStateEnum.Active) {
        current = current.Next();
        yield return current;
      }
    }

    public class TrackConnectionIterator {
      private readonly TrackConnection trackConnection;

      internal TrackConnectionIterator(TrackConnection trackConnection) {
        this.trackConnection = trackConnection ?? throw new ArgumentNullException(nameof(trackConnection));
        TrackConnectionState = TrackConnectionStateEnum.Active;
      }

      private TrackConnectionIterator(TrackConnectionStateEnum trackConnectionState) {
        if(trackConnectionState == TrackConnectionStateEnum.Active) {
          throw new ArgumentException();
        }

        TrackConnectionState = trackConnectionState;
      }

      public enum TrackConnectionStateEnum {
        Active,
        NoneActive,
        NoneAvailable
      }

      public TrackConnectionStateEnum TrackConnectionState { get; }
      public TrackConnection TrackConnection {
        get {
          if (TrackConnectionState != TrackConnectionStateEnum.Active) {
            throw new InvalidOperationException();
          }
          return trackConnection;
        }
      }

      public TrackConnectionIterator Next() {
        var nextSections = TrackConnection.GetNextSections();
        if (nextSections.Count == 0) {
          return new TrackConnectionIterator(TrackConnectionStateEnum.NoneAvailable);
        }
        var nextActiveSection = nextSections.FirstOrDefault(x => x.ViaSection.IsActive);
        if (nextActiveSection == null) {
          return new TrackConnectionIterator(TrackConnectionStateEnum.NoneActive);
        } else {
          return new TrackConnectionIterator(nextActiveSection);
        }
      }
    }
  }
}
