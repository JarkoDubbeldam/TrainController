using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using TrainUI.ViewModels;

namespace TrainUI.Views {
  public class ConnectionStatusView : ReactiveUserControl<ConnectionStatusViewModel> {
    public ConnectionStatusView() {
      this.WhenActivated(disposables => { /* Handle view activation etc. */ });
      this.InitializeComponent();
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
