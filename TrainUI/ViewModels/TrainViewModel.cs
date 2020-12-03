using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using System.Text;

using ReactiveUI;

using TrainUI.Models;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrainViewModel : ReactiveObject, IActivatableViewModel {
    private string name;
    private int speed;
    private short address;
    private ObservableCollection<TrainFunctionViewModel> trainFunctions;

    public ViewModelActivator Activator { get; }

    public TrainViewModel() {
      Activator = new ViewModelActivator();
      TrainFunctions = new ObservableCollection<TrainFunctionViewModel>();
      var hasNonZeroSpeed = this.WhenAnyValue(x => x.Speed, x => x != 0);
      Stop = ReactiveCommand.Create(() => Speed = 0, hasNonZeroSpeed);
      this.WhenActivated((CompositeDisposable disposables) => { 
      });
    }

    [DataMember]
    public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }
    [DataMember]
    public short Address { get => address; set => this.RaiseAndSetIfChanged(ref address, value); }

    public int Speed { get => speed; set => this.RaiseAndSetIfChanged(ref speed, value); }

    [DataMember]
    public ObservableCollection<TrainFunctionViewModel> TrainFunctions { get => trainFunctions; set => this.RaiseAndSetIfChanged(ref trainFunctions, value); }
    [DataMember]
    public Guid Id { get; set; } = Guid.NewGuid();
    public ReactiveCommand<Unit, int> Stop { get; }
    public IEnumerable<TrainFunctionMenuModel> TrainFunctionMenuItems => TrainFunctions.Select(x => new TrainFunctionMenuModel { Id = x.Id, Name = x.Name });

    public void RemoveTrainFunction(Guid id) {
      var function = TrainFunctions.SingleOrDefault(x => x.Id == id);
      if(function != null) {
        TrainFunctions.Remove(function);
      }
    }
  }
}
