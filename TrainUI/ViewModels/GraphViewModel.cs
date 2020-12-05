using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls.Shapes;

using ReactiveUI;

using Splat;

using Z21;

namespace TrainUI.ViewModels {
  public class GraphViewModel : ReactiveObject, IActivatableViewModel {
    private IList<Point> graph;
    private int maxValue;

    public GraphViewModel() {
      Graph = new List<Point>();
      Activator = new ViewModelActivator();
      this.WhenActivated((CompositeDisposable disposables) => {
        var z21Client = Locator.Current.GetService<IZ21Client>();
        Observable.Interval(PollInterval)
          .SelectMany(async _ => {
            try {
              var response = await z21Client.GetSystemState(new Z21.API.SystemStateRequest());
              return response.FilteredMainCurrent;
            } catch (TimeoutException) {
              return 0;
            }
          })
          .Timestamp()
          .AccumulateBuffer(TimeWidth)
          .Subscribe(x => HandleSystemStatusUpdate(x))
          .DisposeWith(disposables);
      });
    }

    public ViewModelActivator Activator { get; }

    public IList<Point> Graph { get => graph; set => this.RaiseAndSetIfChanged(ref graph, value); }
    public int MaxValue { get => maxValue; set => this.RaiseAndSetIfChanged(ref maxValue, value); }
    public int Height => 50;
    public int Width => 150;
    public TimeSpan TimeWidth => TimeSpan.FromMinutes(1);
    public TimeSpan PollInterval => TimeSpan.FromSeconds(1);

    private void HandleSystemStatusUpdate(IReadOnlyList<Timestamped<int>> timestamped) {
      MaxValue = timestamped.Max(x => x.Value);
      var points = timestamped.Select(x => new Point(ScaleDatetime(x.Timestamp.LocalDateTime), ScaleCurrent(x)));
      Graph = points.ToList();
    }

    private double ScaleCurrent(Timestamped<int> x) {
      if(x.Value == 0) {
        return 0;
      }
      return -(double)x.Value / MaxValue * Height;
    }

    private double ScaleDatetime(DateTime timestamp) {
      var now = DateTime.Now;
      var zero = now - TimeWidth;
      var distance = (timestamp - zero).TotalMilliseconds;
      var total = TimeWidth.TotalMilliseconds;
      return distance / total * (Width - 30);   
    }
  }
}
