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
      ViewModel = new ConnectionStatusViewModel();
      this.WhenActivated(disposableRegistration => {
        this.OneWayBind(ViewModel,
                        viewModel => viewModel.Connected,
                        view => view.StatusTextblock.IsVisible).DisposeWith(disposableRegistration);
        this.OneWayBind(ViewModel,
                        viewModel => viewModel.Connected,
                        view => view.StatusCircle.Fill,
                        b => b ? Brushes.Green : Brushes.Red)
        .DisposeWith(disposableRegistration);
      });
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }

    private Ellipse StatusCircle => this.FindControl<Ellipse>(nameof(StatusCircle));
    private TextBlock StatusTextblock => this.FindControl<TextBlock>(nameof(StatusTextblock));
  }
}
