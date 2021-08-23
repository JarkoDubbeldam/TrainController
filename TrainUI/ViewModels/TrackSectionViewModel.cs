using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;

using ReactiveUI;

using Splat;

using Track;

using TrainUI.Models;
using TrainUI.Tools;

using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainUI.ViewModels {
  public class TrackSectionViewModel : ReactiveObject, IActivatableViewModel {
    private PathFigure sectionLine;
    private IPen pen;

    private const int PenWidth = 5;
    private static readonly IPen UnoccupiedPen = new Pen(Brushes.Black, PenWidth, lineCap: PenLineCap.Round);
    private static readonly IPen DisabledPen = new Pen(Brushes.Gray, PenWidth, lineCap: PenLineCap.Round);
    private static readonly IPen OccupiedPen = new Pen(Brushes.DarkRed, PenWidth, lineCap: PenLineCap.Round);
    private bool turnoutsActivated;

    public TrackSectionViewModel() {
      Activator = new ViewModelActivator();

      this.WhenActivated(c => {
        TrackSectionModel.WhenAnyValue(x => x.Boundary1.Location, x => x.Boundary2.Location, x => x.ControlPoint1, x => x.ControlPoint2)
          .Subscribe(HandleSectionUpdate)
          .DisposeWith(c);

        this.WhenAnyValue(x => x.TrackSectionModel.TrackSection.IsOccupied, x => x.TrackSectionModel.TrackSection.IsActive)
          .Subscribe(UpdatePen)
          .DisposeWith(c);
      });


      TurnoutsActivated = true;
      pen = UnoccupiedPen;
    }


    public void ActivateTurnouts() {
      if (TrackSectionModel.TrackSection.IsActive) {
        return;
      }
      var z21client = Locator.Current.GetService<IZ21Client>();
      TrackSectionModel.TrackSection.Activate(z21client);
    }

    private void UpdatePen((bool Occupied, bool Turnouts) status) {
      Dispatcher.UIThread.InvokeAsync(() => {
        if (!status.Turnouts) {
          Pen = DisabledPen;
          return;
        }
        Pen = status.Occupied ? OccupiedPen : UnoccupiedPen;
      });
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
    public bool TurnoutsActivated { get => turnoutsActivated; set => this.RaiseAndSetIfChanged(ref turnoutsActivated, value); }
  }
}
