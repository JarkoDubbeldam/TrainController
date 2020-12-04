using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using System.Text;

using ReactiveUI;

using Z21.Domain;

namespace TrainUI.ViewModels {
  [DataContract]
  public class TrainFunctionViewModel : ReactiveObject, IActivatableViewModel {
    private string name;
    private bool? active;
    private TrainFunctions functionMask;

    public TrainFunctionViewModel() {
      Activator = new ViewModelActivator();
      this.WhenActivated((CompositeDisposable d) => { });
    }

    [DataMember]
    public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }
    public bool? Active { get => active; set => this.RaiseAndSetIfChanged(ref active, value); }

    [DataMember]
    public TrainFunctions FunctionMask { get => functionMask; set => this.RaiseAndSetIfChanged(ref functionMask, value); }
    [DataMember]
    public Guid Id { get; set; } = Guid.NewGuid();

    public ViewModelActivator Activator { get; }

    public void SetTrainFunctionStatus(TrainFunctions trainFunctions) {
      var masked = trainFunctions & FunctionMask;
      if(masked == FunctionMask) {
        Active = true;
      } else if(masked == TrainFunctions.None) {
        Active = false;
      } else {
        Active = null;
      }
    }
  }
}
