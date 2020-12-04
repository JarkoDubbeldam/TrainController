using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

using Avalonia.Media;

using ReactiveUI;

using Splat;

using Z21;

namespace TrainUI.ViewModels {
  public class ConnectionStatusViewModel : ReactiveObject, IActivatableViewModel {
    private bool connected;

    public ConnectionStatusViewModel() {
      Activator = new ViewModelActivator();
      this.WhenActivated((CompositeDisposable disposables) => {
        var z21Client = Locator.Current.GetService<IZ21Client>();
        z21Client.ConnectionStatus
        .Subscribe(x => Connected = x)
        .DisposeWith(disposables);
      });
      this.WhenAnyValue(x => x.Connected).Subscribe(_ => this.RaisePropertyChanged(nameof(CircleColour)));
    }
    public bool Connected { get => connected; set => this.RaiseAndSetIfChanged(ref connected, value); }
    public ISolidColorBrush CircleColour => Connected ? Brushes.Green : Brushes.Red;
    public ViewModelActivator Activator { get; }
  }
}
