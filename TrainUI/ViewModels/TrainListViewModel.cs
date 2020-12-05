using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Runtime.Serialization;

using ReactiveUI;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrainListViewModel : ReactiveObject, IActivatableViewModel {
    private ObservableCollection<TrainViewModel> trains;

    public TrainListViewModel() {
      Trains = new ObservableCollection<TrainViewModel>();
      Activator = new ViewModelActivator();
      this.WhenActivated((CompositeDisposable disposables) => { });
    }

    public ViewModelActivator Activator { get; }

    [DataMember]
    public ObservableCollection<TrainViewModel> Trains { get => trains; set => this.RaiseAndSetIfChanged(ref trains, value); }
  }
}