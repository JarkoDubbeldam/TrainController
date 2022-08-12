using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;

using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

using ReactiveUI;

using Splat;

using Track;

using TrainUI.Models;
using TrainUI.Tools;

using Z21;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrackViewModel : ReactiveObject, IActivatableViewModel {
    private ObservableCollection<TrackSectionViewModel> trackSections;
    private ILookup<IPen, PathFigure> trackSectionFigures;
    private bool editMode;
    private TrackSectionViewModel focus;
    private Rect focusedTrackSectionRect;
    private List<EllipseGeometry> focusedTrackSectionCircles;
    private string trackPiecesJson;

    public TrackViewModel() {
      Activator = new ViewModelActivator();

      this.WhenActivated(c => {
        this.WhenAnyValue(x => x.TrackSections)
          .Subscribe(x => {
            foreach (var item in x) {
              item.Activator.Activate().DisposeWith(c);
              item.WhenAnyValue(x => x.TrackSectionModel.ControlPoint1,
                x => x.TrackSectionModel.ControlPoint2,
                x => x.TrackSectionModel.Boundary1.Location,
                x => x.TrackSectionModel.Boundary2.Location,
                x => x.Pen)
                .Subscribe(_ => {
                  RecalculateFigures();
                  RecalculateFocusFigures();
                })
                .DisposeWith(c);
            }
          })
          .DisposeWith(c);
        this.WhenAnyValue(x => x.Focus)
          .Subscribe(_ => RecalculateFocusFigures())
          .DisposeWith(c);
        this.WhenAnyValue(x => x.EditMode)
          .Where(x => !x)
          .Subscribe(x => Focus = null)
          .DisposeWith(c);

        this.WhenAnyValue(x => x.TrackPiecesJson)
          .Skip(1)
          .DistinctUntilChanged()
          .Subscribe(LoadTrackPieces)
          .DisposeWith(c);

        var client = Locator.Current.GetService<IZ21Client>();
        //TrackRepository.SetupSubscriptions(client).DisposeWith(c);
      });
      TrackSections = new ObservableCollection<TrackSectionViewModel>();
      TrackSectionFigures = null;
      FocusedTrackSectionCircles = new List<EllipseGeometry>();
    }

    public TrackSectionViewModel FindTrackSection(Point position) {
      var closest = TrackSections.Select(trackSection => new {
        TrackSection = trackSection,
        Distance = new CubicBezierSegment {
          Control1 = trackSection.TrackSectionModel.ControlPoint1,
          Control2 = trackSection.TrackSectionModel.ControlPoint2,
          End = trackSection.TrackSectionModel.Boundary2.Location
        }.GetMinimalDistance(trackSection.TrackSectionModel.Boundary1.Location, position, 101)
      })
        .OrderBy(x => x.Distance)
        .FirstOrDefault();
      if (closest?.Distance <= 15) {
        return closest.TrackSection;
      } else {
        return null;
      }
    }

    private void RecalculateFigures() {
      Dispatcher.UIThread.InvokeAsync(() => {
        var lookup = TrackSections.ToLookup(x => x.Pen, x => new PathFigure {
          IsClosed = false,
          StartPoint = x.TrackSectionModel.Boundary1.Location,
          Segments = new PathSegments {
            new CubicBezierSegment {
              End = x.TrackSectionModel.Boundary2.Location,
              Control1 = x.TrackSectionModel.ControlPoint1,
              Control2 = x.TrackSectionModel.ControlPoint2
            }
          }
        });
        TrackSectionFigures = lookup;
      });
    }

    private void RecalculateFocusFigures() {
      using var holdOn = DelayChangeNotifications();
      if (Focus == null) {
        FocusedTrackSectionRect = Rect.Empty;
        FocusedTrackSectionCircles = new List<EllipseGeometry>();
        return;
      }
      const int margin = 10;
      var points = new[] {
        Focus.TrackSectionModel.Boundary1.Location,
        Focus.TrackSectionModel.Boundary2.Location,
        Focus.TrackSectionModel.ControlPoint1,
        Focus.TrackSectionModel.ControlPoint2
      };
      var minX = points.Min(x => x.X);
      var maxX = points.Max(x => x.X);
      var minY = points.Min(x => x.Y);
      var maxY = points.Max(x => x.Y);
      FocusedTrackSectionRect = new Rect(new Point(minX - margin, minY - margin), new Point(maxX + margin, maxY + margin));

      const int circleSize = 5;
      FocusedTrackSectionCircles = points.Select(x => new EllipseGeometry {
        Rect = new Rect(new Point(x.X - circleSize, x.Y - circleSize), new Point(x.X + circleSize, x.Y + circleSize))
      }).ToList();

    }

    public bool TryMatchFocusedPoints(Point clickPosition, out DataObject dataObject) {
      dataObject = null;
      if (focus == null) {
        return false;
      }
      var points = new[] {
        Focus.TrackSectionModel.Boundary1.Location,
        Focus.TrackSectionModel.Boundary2.Location,
        Focus.TrackSectionModel.ControlPoint1,
        Focus.TrackSectionModel.ControlPoint2
      };

      var closestPoint = points.Select((x, idx) => new {
        Point = x,
        Type = (PointType)idx,
        Distance = Utils.Distance(x, clickPosition)
      })
        .Where(x => x.Distance <= 5)
        .OrderBy(x => x.Distance)
        .FirstOrDefault();
      if (closestPoint == null) {
        return false;
      }
      dataObject = new DataObject();
      dataObject.Set("PointType", closestPoint.Type);
      return true;
    }

    public void UpdatePoint(Point pointerPosition, IDataObject data) {
      switch ((PointType)data.Get("PointType")) {
        case PointType.Boundary1:
          Focus.TrackSectionModel.Boundary1.Location = pointerPosition;
          break;
        case PointType.Boundary2:
          Focus.TrackSectionModel.Boundary2.Location = pointerPosition;
          break;
        case PointType.ControlPoint1:
          focus.TrackSectionModel.ControlPoint1 = pointerPosition;
          break;
        case PointType.ControlPoint2:
          focus.TrackSectionModel.ControlPoint2 = pointerPosition;
          break;
      }
    }

    private enum PointType {
      Boundary1,
      Boundary2,
      ControlPoint1,
      ControlPoint2
    }


    private void LoadTrackPieces(string json) {
      var repos = Locator.Current.GetService<Func<string, TrackRepository>>()(json);
      var random = new Random();
      var boundaries = repos.Boundaries.ToDictionary(x => x.Id, x => new TrackSectionBoundaryModel { Id = x.Id, Location = new Point(random.Next(100, 200), random.Next(100, 200)) });
            
      using var _ = this.DelayChangeNotifications();

      Focus = null;
      TrackSections = new ObservableCollection<TrackSectionViewModel>(repos.Sections.Select(x => new TrackSectionViewModel {
        TrackSectionModel = new TrackSectionModel {
          Boundary1 = boundaries[x.ConnectedBoundaries[0].Id],
          Boundary2 = boundaries[x.ConnectedBoundaries[1].Id],
          ControlPoint1 = new Point(random.Next(100, 200), random.Next(100, 200)),
          ControlPoint2 = new Point(random.Next(100, 200), random.Next(100, 200)),
          TrackSection = x
        }
      }));
    }


    public ViewModelActivator Activator { get; }
    public ObservableCollection<TrackSectionViewModel> TrackSections { get => trackSections; set => this.RaiseAndSetIfChanged(ref trackSections, value); }
    public ILookup<IPen, PathFigure> TrackSectionFigures { get => trackSectionFigures; set => this.RaiseAndSetIfChanged(ref trackSectionFigures, value); }
    public Rect FocusedTrackSectionRect { get => focusedTrackSectionRect; set => this.RaiseAndSetIfChanged(ref focusedTrackSectionRect, value); }
    public List<EllipseGeometry> FocusedTrackSectionCircles { get => focusedTrackSectionCircles; set => this.RaiseAndSetIfChanged(ref focusedTrackSectionCircles, value); }
    public TrackSectionViewModel Focus { get => focus; set => this.RaiseAndSetIfChanged(ref focus, value); }
    public bool EditMode { get => editMode; set => this.RaiseAndSetIfChanged(ref editMode, value); }
    public string TrackPiecesJson { get => trackPiecesJson; set => this.RaiseAndSetIfChanged(ref trackPiecesJson, value); }
    public TrackRepository TrackRepository { get; internal set; }
  }
}
