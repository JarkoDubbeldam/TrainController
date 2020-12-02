using Autofac;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using TrainUI.ViewModels;

using Z21;

namespace TrainUI.Views {
  public class MainWindow : ReactiveWindow<MainWindowViewModel> {
    public MainWindow() {
      ViewModel = new MainWindowViewModel();
      InitializeComponent();
      this.WhenActivated(disposables => { 
        /* Handle view activation etc. */ });
#if DEBUG
      this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
