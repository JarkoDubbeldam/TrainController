using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;

using ReactiveUI;

using Track;

namespace TrainUI.Models {
  public class TrackSectionModel : ReactiveObject {
    private TrackSectionBoundaryModel boundary1;
    private TrackSectionBoundaryModel boundary2;
    private Point controlPoint1;
    private Point controlPoint2;

    public TrackSectionBoundaryModel Boundary1 { get => boundary1; set => this.RaiseAndSetIfChanged(ref boundary1, value); }

    public TrackSectionBoundaryModel Boundary2 { get => boundary2; set => this.RaiseAndSetIfChanged(ref boundary2, value); }

    public Point ControlPoint1 { get => controlPoint1; set => this.RaiseAndSetIfChanged(ref controlPoint1, value); }
    public Point ControlPoint2 { get => controlPoint2; set => this.RaiseAndSetIfChanged(ref controlPoint2, value); }
    public TrackSection TrackSection { get; set; }
  }
}
