using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using ReactiveUI;

using TrainUI.Models;

namespace TrainUI.ViewModels {
  [DataContract]
  public class MainWindowViewModel : ReactiveObject, IActivatableViewModel {


    public MainWindowViewModel(MainWindowModel model, Func<TrainModel, TrainViewModel> trainViewModelFactory, ConnectionStatusViewModel connectionStatus) {
      Trains = new ObservableCollection<TrainViewModel>(model.Trains.Select(trainViewModelFactory));
      ConnectionStatus = connectionStatus;
      ConnectionStatus.WhenAnyValue(x => x.Connected).DistinctUntilChanged().Where(x => x).Subscribe(x => OnConnected());
      this.WhenActivated(() => new IDisposable[] {
      });
    }

    private Task OnConnected() {
      return Task.WhenAll(Trains.Select(x => x.Connect()));
    }

    [IgnoreDataMember]
    public ViewModelActivator Activator => new ViewModelActivator();

    [DataMember]
    public ObservableCollection<TrainViewModel> Trains { get; }

    public ConnectionStatusViewModel ConnectionStatus { get; }
  }
}
