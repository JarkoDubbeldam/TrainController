using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Media;

using ReactiveUI;

using Z21;

namespace TrainUI.ViewModels {
  public class ConnectionStatusViewModel : ReactiveObject, IActivatableViewModel {
    private readonly IZ21Client z21Client;
    private bool connected = false;

    public ConnectionStatusViewModel(IZ21Client z21Client) {
      this.z21Client = z21Client;

      Observable.Interval(TimeSpan.FromSeconds(2)).SelectMany(CheckConnectionStatus).DistinctUntilChanged().Subscribe(UpdateConnectionStatus);
      this.WhenActivated(() => new IDisposable[]{
      });
    }

    public bool Connected { get => connected; set => this.RaiseAndSetIfChanged(ref connected, value); }
    public IBrush CircleColor => connected ? Brushes.Green : Brushes.Red;

    private void UpdateConnectionStatus(bool isConnected) {
      Connected = isConnected;
      this.RaisePropertyChanged(nameof(CircleColor));
    }

    private async Task<bool> CheckConnectionStatus(long _) {
      try {
        await z21Client.GetSystemState(new Z21.API.SystemStateRequest { });
        return true;
      } catch (TimeoutException) {
        return false;
      }
    }

    public ViewModelActivator Activator => new ViewModelActivator();
  }
}
