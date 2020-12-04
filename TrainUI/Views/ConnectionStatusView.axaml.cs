using System.Reactive.Disposables;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using ReactiveUI;

using TrainUI.ViewModels;

namespace TrainUI.Views {
  public class ConnectionStatusView : ReactiveUserControl<ConnectionStatusViewModel> {
    public ConnectionStatusView() {
      this.InitializeComponent();
      this.WhenActivated(c => {
        //this.OneWayBind(ViewModel,
        //          viewModel => viewModel.Connected,
        //          view => view.StatusCircle.Fill,
        //          b => b ? Brushes.Green : Brushes.Red)
        //    .DisposeWith(c);
      });
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }

    private Ellipse StatusCircle => this.FindControl<Ellipse>(nameof(StatusCircle));
  }
}
