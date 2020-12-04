using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using System.Text;

using ReactiveUI;

namespace TrainUI.ViewModels {
  [DataContract]
  public class MainWindowViewModel : ReactiveObject, IActivatableViewModel {
    private ReactiveObject content;
    private ConnectionStatusViewModel connectionStatus;

    public MainWindowViewModel() {
      Activator = new ViewModelActivator();
      Content = new TrainOverviewViewModel();
      ConnectionStatus = new ConnectionStatusViewModel();
      this.WhenActivated((CompositeDisposable d) => {
      });
    }

    [DataMember]
    public ReactiveObject Content { get => content; set => this.RaiseAndSetIfChanged(ref content, value); }
    public ConnectionStatusViewModel ConnectionStatus { get => connectionStatus; set => this.RaiseAndSetIfChanged(ref connectionStatus, value); }

    public ViewModelActivator Activator { get; }
  }
}
