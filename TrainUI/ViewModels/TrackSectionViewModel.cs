using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

using Avalonia;
using Avalonia.Media;

using ReactiveUI;

using Splat;

using TrainUI.Models;
using TrainUI.Tools;

using Z21;

namespace TrainUI.ViewModels {
  public class TrackSectionViewModel : ReactiveObject, IActivatableViewModel {
    private PathFigure sectionLine;
    private IPen pen;
    private bool occupied;

    public TrackSectionViewModel() {
      Activator = new ViewModelActivator();

      this.WhenActivated(c => {
        TrackSectionModel.WhenAnyValue(x => x.Boundary1.Location, x => x.Boundary2.Location, x => x.ControlPoint1, x => x.ControlPoint2)
          .Subscribe(HandleSectionUpdate)
          .DisposeWith(c);
        var z21client = Locator.Current.GetService<IZ21Client>();
        z21client.OccupancyStatusChanged
          .Subscribe(o => Occupied = o.Occupancies[TrackSectionModel.SectionId])
          .DisposeWith(c);

        this.WhenAnyValue(x => x.Occupied)
          .Subscribe(x => Pen = new Pen(x ? Brushes.Red : Brushes.Black, 2))
          .DisposeWith(c);
      });

   

      pen = new Pen(Brushes.Black, 2);
    }

    private void HandleSectionUpdate((Point Location1, Point Location2, Point Control1, Point Control2) points) {
      SectionLine = new PathFigure {
        IsClosed = false,
        StartPoint = points.Location1,
        Segments = new PathSegments {
          new CubicBezierSegment {
            Control1 = points.Control1,
            Control2 = points.Control2,
            End = points.Location2
          }
        }
      };
    }

    public ViewModelActivator Activator { get; }

    public TrackSectionModel TrackSectionModel { get; set; }

    public PathFigure SectionLine { get => sectionLine; set => this.RaiseAndSetIfChanged(ref sectionLine, value); }
    public IPen Pen { get => pen; set => this.RaiseAndSetIfChanged(ref pen, value); }
    public bool Occupied { get => occupied; set => this.RaiseAndSetIfChanged(ref occupied, value); }
  }
}
