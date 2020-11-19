using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using TrainUI.ViewModels;

namespace TrainUI.Views {
  public class MainWindow : ReactiveWindow<MainWindowViewModel> {
    public MainWindow() {
      InitializeComponent();
      this.WhenActivated(disposables => { /* Handle view activation etc. */ });
#if DEBUG
      this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
