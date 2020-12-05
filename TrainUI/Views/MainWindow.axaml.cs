using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using TrainUI.ViewModels;

namespace TrainUI.Views {
  public class MainWindow : ReactiveWindow<MainWindowViewModel> {
    public MainWindow() {
      this.InitializeComponent();
#if DEBUG
      this.AttachDevTools();
#endif
      this.WhenActivated(disposables => { 
      });
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
