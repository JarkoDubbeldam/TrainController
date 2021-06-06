using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using ReactiveUI;

using TrainUI.ViewModels;

namespace TrainUI.Views {
  public partial class TrackView : ReactiveUserControl<TrackViewModel> {
    static TrackView() {
      AffectsRender<TrackView>(TrackFiguresProperty, FocusedTrackSectionRectProperty, FocusedTrackSectionCirclesProperty);
    }

    public TrackView() {
      InitializeComponent();
      this.WhenActivated(c => {
        this.Find<Button>("Load").Click += HandleLoadClick;
      });

    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }

    public static StyledProperty<ILookup<IPen, PathFigure>> TrackFiguresProperty =
      AvaloniaProperty.Register<TrackView, ILookup<IPen, PathFigure>>(nameof(TrackFigures));
    public ILookup<IPen, PathFigure> TrackFigures { get => GetValue(TrackFiguresProperty); set => SetValue(TrackFiguresProperty, value); }
    public static StyledProperty<Rect> FocusedTrackSectionRectProperty =
      AvaloniaProperty.Register<TrackView, Rect>(nameof(FocusedTrackSectionRect));
    public Rect FocusedTrackSectionRect { get => GetValue(FocusedTrackSectionRectProperty); set => SetValue(FocusedTrackSectionRectProperty, value); }

    public static StyledProperty<List<EllipseGeometry>> FocusedTrackSectionCirclesProperty =
      AvaloniaProperty.Register<TrackView, List<EllipseGeometry>>(nameof(FocusedTrackSectionCircles));
    public List<EllipseGeometry> FocusedTrackSectionCircles { get => GetValue(FocusedTrackSectionCirclesProperty); set => SetValue(FocusedTrackSectionCirclesProperty, value); }


    public override void Render(DrawingContext context) {
      base.Render(context);

      foreach(var grouping in TrackFigures ?? Enumerable.Empty<IGrouping<IPen, PathFigure>>()) {
        var pen = grouping.Key;
        var figures = new PathFigures();
        figures.AddRange(grouping);
        context.DrawGeometry(Brushes.Transparent, pen, new PathGeometry { Figures = figures });
      }

      if (FocusedTrackSectionRect != Rect.Empty) {
        var focusRectPen = new Pen(Brushes.Black, 1, DashStyle.Dash);
        context.DrawRectangle(focusRectPen, FocusedTrackSectionRect);
      }
      foreach (var ellipse in FocusedTrackSectionCircles) {
        var focusPen = new Pen(Brushes.Red, 1);
        context.DrawGeometry(Brushes.Transparent, focusPen, ellipse);
      }
    }

    public async void OnPointerPressed(object sender, PointerPressedEventArgs args) {
      var canvas = this.Find<Canvas>("Map");
      var position = args.GetPosition(canvas);
      if (!ViewModel.EditMode) {
        var section = ViewModel.FindTrackSection(position);
        if(section != null) {
          section.ActivateTurnouts();
        }
      } else {
        if (ViewModel.TryMatchFocusedPoints(position, out var dataObject)) {
          // do stuff;
          void UpdatePoint(object sender, DragEventArgs e) {
            var pointerPosition = e.GetPosition(canvas);
            ViewModel.UpdatePoint(pointerPosition, e.Data);
            e.Handled = true;
          }
          using var _ = AddHandler(DragDrop.DragOverEvent, UpdatePoint);
          await DragDrop.DoDragDrop(args, dataObject, DragDropEffects.Move);
          return;
        }

        ViewModel.Focus = ViewModel.FindTrackSection(position);
      }
      args.Handled = true;
    }

    public async void HandleLoadClick(object sender, EventArgs e) {
      var dialog = new OpenFileDialog {
        AllowMultiple = false,
        Filters = new List<FileDialogFilter> {
          new FileDialogFilter {
            Name = "Json files",
            Extensions = new List<string> {
              "json"
            }
          }
        }
      };

      var filename = await dialog.ShowAsync((Window)this.VisualRoot);
      if (filename.Length == 1) {
        ViewModel.TrackPiecesJson = File.ReadAllText(filename[0]);
      }
    }
  }
}
