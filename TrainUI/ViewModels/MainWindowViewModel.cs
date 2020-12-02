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

using Autofac;

using ReactiveUI;

using Splat;

using TrainUI.Models;

using Z21;

namespace TrainUI.ViewModels {
  [DataContract]
  public class MainWindowViewModel : ReactiveObject, IActivatableViewModel {

    public MainWindowViewModel() {
      ConnectionStatus = new ConnectionStatusViewModel(Locator.Current.GetService<IZ21Client>());
      this.WhenActivated(() => new IDisposable[] {

      });
    }


    public MainWindowViewModel(MainWindowModel model, Func<TrainModel, TrainViewModel> trainViewModelFactory) {
      Trains = new ObservableCollection<TrainViewModel>(model.Trains.Select(trainViewModelFactory));
      //ConnectionStatus = connectionStatus;
      //ConnectionStatus.WhenAnyValue(x => x.Connected).DistinctUntilChanged().Where(x => x).Subscribe(x => OnConnected());
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
