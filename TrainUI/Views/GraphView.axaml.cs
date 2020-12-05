using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

using TrainUI.ViewModels;

namespace TrainUI.Views {
  public class GraphView : ReactiveUserControl<GraphViewModel> {
    public GraphView() {
      this.InitializeComponent();
      this.WhenActivated(c => { });
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
