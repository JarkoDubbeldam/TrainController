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
    private bool occupied;
    private List<TurnoutModel> turnouts;

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
        var z21client = Locator.Current.GetService<IZ21Client>();
        z21client.OccupancyStatusChanged
          .Subscribe(o => Occupied = o.Occupancies[TrackSectionModel.TrackSection.SectionId])
          .DisposeWith(c);

        this.WhenAnyValue(x => x.Occupied, x=> x.TurnoutsActivated)
          .Subscribe(UpdatePen)
          .DisposeWith(c);

        this.turnouts = TrackSectionModel.TrackSection.Turnouts
          .Select(x => new TurnoutModel {
            Id = x.TurnoutId,
            Mode = (TurnoutMode?)null
          })
          .ToList();
        foreach(var turnoutPosition in TrackSectionModel.TrackSection.Turnouts) {
          z21client.GetTurnoutInformation(new Z21.API.TurnoutInformationRequest { Address = (short)turnoutPosition.TurnoutId })
            .ToObservable()
            .Subscribe(HandleTurnoutStatus)
            .DisposeWith(c);
          z21client.TurnoutInformationChanged
            .Where(x => x.Address == turnoutPosition.TurnoutId)
            .Subscribe(x => {
              HandleTurnoutStatus(x);
              UpdateTurnoutStatus();
            })
            .DisposeWith(c);
        }
      });


      TurnoutsActivated = true;
      pen = UnoccupiedPen;
    }

    private void HandleTurnoutStatus(TurnoutInformation turnoutInformation) {
      this.turnouts.Single(y => y.Id == turnoutInformation.Address).Mode = turnoutInformation.TurnoutPosition switch {
        TurnoutPosition.Position1 => TurnoutMode.Right,
        TurnoutPosition.Position2 => TurnoutMode.Left,
        _ => null
      };
    }

    public void ActivateTurnouts() {
      var z21client = Locator.Current.GetService<IZ21Client>();
      Observable.Interval(TimeSpan.FromMilliseconds(200))
        .Take(TrackSectionModel.TrackSection.Turnouts.Count)
        .Zip(TrackSectionModel.TrackSection.Turnouts)
        .Subscribe(x => z21client.SetTurnout(new SetTurnoutRequest {
          Address = (short)x.Second.TurnoutId,
          TurnoutPosition = x.Second.TurnoutMode switch {
            TurnoutMode.Right => TurnoutPosition.Position1,
            TurnoutMode.Left => TurnoutPosition.Position2,
            _ => throw new ArgumentOutOfRangeException()
          },
          Activation = Activation.Activate,
          QueueMode = true
        }));
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

    private async void UpdateTurnoutStatus() {
      TurnoutsActivated = turnouts.Zip(TrackSectionModel.TrackSection.Turnouts, (actual, required) => (actual.Mode, required.TurnoutMode) switch {
        (null, _) => true,
        (TurnoutMode.Left, TurnoutMode.Left) => true,
        (TurnoutMode.Right, TurnoutMode.Right) => true,
        _ => false
      }).All(x => x);
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
    public bool TurnoutsActivated { get => turnoutsActivated; set => this.RaiseAndSetIfChanged(ref turnoutsActivated, value); }
  }
}
